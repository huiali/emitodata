using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Huiali.ILOData.Extensions;
using Huiali.ILOData.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Huiali.ILOData
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddOData();

            var connectionSetion = Configuration.GetSection("ConnectionStrings");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(app.ApplicationServices);

            var connections = this.Configuration.GetSection("ConnectionStrings").GetChildren();
            Action<IRouteBuilder> configureRoutes = routeBuilder =>
            {
                foreach (var item in connections)
                {
                    var connectionString = item.Value;
                    var tables = DbSchemaReader.GetSchemata(connectionString);

                    //Multipl entitySet from tables
                    //builder.EntitySet<>()

                    routeBuilder.MapODataServiceRoute(
                        $"ODATAROUTE_{item.Key}",
                        item.Key,
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
