using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Huiali.EmitOData.Extensions;
using Huiali.EmitOData.Emit;
using Huiali.EmitOData.Models;
using Microsoft.Extensions.Logging;

namespace Huiali.ILOData
{
    public class Startup
    {
        private IEnumerable<IConfigurationSection> Connections { get; }
        private Dictionary<string, List<Entry>> _entrys = new Dictionary<string, List<Entry>>();
        public Startup(IConfiguration configuration)
        {
            this.Connections = configuration.GetSection("ConnectionStrings").GetChildren();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            ModuleBuilder moduleBuilder = ClrTypeBuilder.GetModuleBuilder();
            List<Type> connectionTypes = new List<Type>();
            foreach (var connection in this.Connections)
            {
                var connectionString = connection.Value;
                var tables = DbSchemaReader.GetSchemata(connectionString);
                List<Entry> entrys = new List<Entry>();
                foreach (var table in tables)
                {
                    var modelType = moduleBuilder.CreateModelType(connection.Key, table);
                    entrys.Add(new Entry(modelType, table));
                }
                _entrys.Add(connection.Key, entrys);
                var dbcontextType = moduleBuilder.CreateDbContextType(connection.Key, entrys);
                Action<DbContextOptionsBuilder> optionsAction = options => options.UseSqlServer(connection.Value);
                MethodInfo addDbContextmethod =
                    typeof(EntityFrameworkServiceCollectionExtensions).
                        GetMethods(BindingFlags.Public | BindingFlags.Static).
                        FirstOrDefault(mi => mi.Name == "AddDbContext" && mi.GetGenericArguments().Count() == 1).
                        MakeGenericMethod(new Type[] { dbcontextType });
                addDbContextmethod.Invoke(null, new object[] { services, optionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
                foreach (var entry in entrys)
                {
                    var controllerType = moduleBuilder.CreateControllerType(
                        connection.Key,
                        entry.Type,
                        dbcontextType);

                    connectionTypes.Add(controllerType);
                }
            }

            services.AddMvc().
                SetCompatibilityVersion(CompatibilityVersion.Version_2_1).
                ConfigureApplicationPartManager(p => p.FeatureProviders.Add(new EmitTypeControllerFeatureProvider(connectionTypes)));
            services.AddOData();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();
            Action<IRouteBuilder> configureRoutes = routeBuilder =>
            {
                foreach (var Connection in this.Connections)
                {
                    ODataConventionModelBuilder builder = new ODataConventionModelBuilder(app.ApplicationServices);
                    var entrys = _entrys[Connection.Key];

                    foreach (Entry entry in entrys)
                    {
                        var entityType = builder.AddEntityType(entry.Type);
                        var entirySet=builder.AddEntitySet(entry.Type.Name, entityType);
                    }
                    var edmModel = builder.GetEdmModel();
                    routeBuilder.MapODataServiceRoute($"ODATAROUTE_{Connection.Key}", Connection.Key, edmModel);
                }

                routeBuilder
                    .Count()
                    .Filter()
                    .OrderBy()
                    .Expand()
                    .Select()
                    .MaxTop(null);
                routeBuilder.EnableDependencyInjection();
            };
            app.UseMvc(configureRoutes);
            app.UseDeveloperExceptionPage();

        }
    }
}
