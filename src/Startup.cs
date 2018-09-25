using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Huiali.ILOData.Extensions;
using Huiali.ILOData.ILEmit;
using Huiali.ILOData.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Huiali.ILOData
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IEnumerable<IConfigurationSection> Connections;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.Connections = this.Configuration.GetSection("ConnectionStrings").GetChildren();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddOData();

            foreach (var item in this.Connections)
            {
                //services.AddDbContext<>(options => options.UseSqlServer(item.Value))
            }
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(app.ApplicationServices);

            var assemblyName = "Huiali.ILOData.ILEmit";
            var modelName = new AssemblyName(assemblyName);
            var dynamicModelAssembly = AssemblyBuilder.DefineDynamicAssembly(modelName, AssemblyBuilderAccess.RunAndCollect);
            var modelBuilder = dynamicModelAssembly.DefineDynamicModule(modelName.Name);

            Action<IRouteBuilder> configureRoutes = routeBuilder =>
            {
                foreach (var Connection in this.Connections)
                {
                    var connectionString = Connection.Value;
                    var tables = DbSchemaReader.GetSchemata(connectionString);
                    List<Type> modelTypes=new List<Type>();
                    foreach (var table in tables)
                    {
                        var modelType = modelBuilder.CreateModelType($"{assemblyName}.{Connection.Key}.Models", table);
                        modelTypes.Add(modelType);
                        //builder.EntitySet<>()
                        var entityType = builder.AddEntityType(modelType);
                        builder.AddEntitySet(table.Name, entityType);
                    }

                    var dbcontextType = modelBuilder.CreateDbContext($"{assemblyName}.{Connection.Key}.Models.{Connection.Key}Context", modelTypes);

                    
                    routeBuilder.MapODataServiceRoute(
                        $"ODATAROUTE_{Connection.Key}",
                        Connection.Key,
                        builder.GetEdmModel());
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
