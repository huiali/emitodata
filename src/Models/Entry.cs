using System;
using System.Collections.Generic;

namespace Huiali.EmitOData.Models
{
    public class Entry
    {
        public Entry(Type modeltype, Table table)
        {
            this.Type = modeltype;
            this.Table = table;
        }

        public Type Type { set; get; }
        public Table Table { set; get; }
    }
}