using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;

namespace Huiali.ILOData.Models
{
    public class ODataRouteModel
    {
       internal string routeName { get; set; }
       internal string routePrefix { get; set; }
       internal IEdmModel edmModel { get; set; }
    }
}