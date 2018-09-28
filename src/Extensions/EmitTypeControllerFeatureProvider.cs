using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Huiali.EmitOData.Extensions
{
    public class EmitTypeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IEnumerable<Type> _controllerTypes;
        public EmitTypeControllerFeatureProvider(IEnumerable<Type> controllerTypes)
        {
            this._controllerTypes = controllerTypes;
        }
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var itemType in this._controllerTypes)
            {
                feature.Controllers.Add(itemType.GetTypeInfo());
            }
        }
    }
}