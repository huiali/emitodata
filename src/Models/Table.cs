using System;
using System.Collections.Generic;

namespace Huiali.ILOData.Models
{
    public class Table
    {
        public string Name { set; get; }
        public IEnumerable<Column> Columns { set; get; }
    }
}