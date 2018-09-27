using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Huiali.ILOData.ILEmit;
using Huiali.ILOData.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.OData.Edm;

namespace Huiali.ILOData.Extensions
{
    public class EdmModelBuilder
    {
        public IEdmModel GetEdmModel(IConfigurationSection Connection, IServiceProvider serviceProvider, ModuleBuilder moduleBuilder)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(serviceProvider);
            var connectionString = Connection.Value;
            var tables = DbSchemaReader.GetSchemata(connectionString);
            List<Type> modelTypes = new List<Type>();
            foreach (var table in tables)
            {
                var modelType = moduleBuilder.CreateModelType($"{moduleBuilder.Name}.{Connection.Key}.Models", table);
                modelTypes.Add(modelType);
                //builder.EntitySet<>()
                var entityType = builder.AddEntityType(modelType);
                builder.AddEntitySet(table.Name, entityType);
            }
            return builder.GetEdmModel();
        }
    }
}