using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
            services.AddTransient<EdmModelBuilder>();
            // foreach (var item in this.Connections)
            // {
            //     //services.AddDbContext<>(options => options.UseSqlServer(item.Value))
            //     Action<DbContextOptionsBuilder> optionsAction = options => options.UseSqlServer(item.Value);
            //     MethodInfo addDbContextmethod = services
            //     .GetType()
            //     .GetMethod("AddDbContext")
            //     .MakeGenericMethod(new Type[] { });

            //     addDbContextmethod.Invoke(null, new object[] { optionsAction });
            // }

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, EdmModelBuilder eb)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(app.ApplicationServices);
            ModuleBuilder moduleBuilder = ClrTypeBuilder.GetModuleBuilder();

            // AssemblyName aName = new AssemblyName("DynamicAssemblyExample");
            //         AssemblyBuilder ab = 
            //             AppDomain.CurrentDomain.DefineDynamicAssembly(
            //                 aName, 
            //                 AssemblyBuilderAccess.RunAndSave);



            Action<IRouteBuilder> configureRoutes = routeBuilder =>
            {
                foreach (var Connection in this.Connections)
                {
                    var edmModel = eb.GetEdmModel(Connection, app.ApplicationServices, moduleBuilder);
                    routeBuilder.MapODataServiceRoute($"ODATAROUTE_{Connection.Key}", Connection.Key, edmModel);
                    
                    
                    // var dbcontextType = moduleBuilder.CreateDbContext($"{moduleBuilder.Name}.{Connection.Key}.Models.{Connection.Key}Context", modelTypes);
                    // foreach (var itemType in modelTypes)
                    // {
                    //     var controllerType = modelBuilder.CreateControllerType(
                    //         $"{assemblyName}.{Connection.Key}.Controllers.{itemType.Name}Controller",
                    //          itemType,
                    //          dbcontextType);
                    // }
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
