using System;
using System.Collections.Generic;

namespace Huiali.EmitOData.Models
{
    public class Table
    {
        public string Schema { set; get; }
        public string Name { set; get; }
        public IEnumerable<Column> Columns { set; get; }
    }
}