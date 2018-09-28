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

namespace Huiali.ILOData
{
    public class Startup
    {
        private IEnumerable<IConfigurationSection> Connections { get; }
        private Dictionary<string, List<Type>> _modelTypes = new Dictionary<string, List<Type>>();
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
                List<Type> modelTypes = new List<Type>();
                foreach (var table in tables)
                {
                    var modelType = moduleBuilder.CreateModelType(connection.Key, table);
                    modelTypes.Add(modelType);
                }
                _modelTypes.Add(connection.Key, modelTypes);
                var dbcontextType = moduleBuilder.CreateDbContextType(connection.Key, modelTypes);
                Action<DbContextOptionsBuilder> optionsAction = options => options.UseSqlServer(connection.Value);
                MethodInfo addDbContextmethod =
                    typeof(EntityFrameworkServiceCollectionExtensions).
                        GetMethods(BindingFlags.Public | BindingFlags.Static).
                        FirstOrDefault(mi => mi.Name == "AddDbContext" && mi.GetGenericArguments().Count() == 1).
                        MakeGenericMethod(new Type[] { dbcontextType });
                addDbContextmethod.Invoke(null, new object[] { services, optionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
                foreach (var itemType in modelTypes)
                {
                    var controllerType = moduleBuilder.CreateControllerType(
                        connection.Key,
                        itemType,
                        dbcontextType);

                    connectionTypes.Add(controllerType);
                }

                services.AddMvc().
                SetCompatibilityVersion(CompatibilityVersion.Version_2_1).
                ConfigureApplicationPartManager(p => p.FeatureProviders.Add(new EmitTypeControllerFeatureProvider(connectionTypes)));
                services.AddOData();
            }
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Action<IRouteBuilder> configureRoutes = routeBuilder =>
            {
                foreach (var Connection in this.Connections)
                {
                    ODataConventionModelBuilder builder = new ODataConventionModelBuilder(app.ApplicationServices);
                    var models = _modelTypes[Connection.Key];

                    foreach (Type modelType in models)
                    {
                        var entityType = builder.AddEntityType(modelType);
                        builder.AddEntitySet(modelType.Name, entityType);
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
