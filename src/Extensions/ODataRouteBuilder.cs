using System;
using System.Collections.Generic;
using Huiali.ILOData.Models;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Routing;

namespace Huiali.ILOData.Extensions
{
    public static class ODataRouteBuilder
    {
        public static IEnumerable<ODataRoute> MapODataServiceRoutes(this IRouteBuilder routeBuilder, IEnumerable<ODataRouteModel> oDataRouteModels)
        {
            foreach (var item in oDataRouteModels)
            {
                yield return routeBuilder.MapODataServiceRoute(item.routeName, item.routePrefix, item.edmModel);
            }
        }
    }
}