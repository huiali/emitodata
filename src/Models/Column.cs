using System;

namespace Huiali.ILOData.Models
{
    public class Column
    {
        public string SchemaName{set;get;}
        public string TableName{set;get;}
        public string ColumnName { set; get; }
        public Type ColumnType { set; get; }
        public bool IsPrimaryKey { set; get; }
        public bool IsNullable { set; get; }
        public bool IsIdentity { set; get; }
        public int MaxLength{set;get;}
    }
}