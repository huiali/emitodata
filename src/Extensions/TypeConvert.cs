using System;

namespace Huiali.ILOData.Extensions
{
    public class TypeConvert
    {
        public static Type GetRuntimeType(string dataType, bool isNullable)
        {
            switch (dataType)
            {
                case "int":
                    return isNullable ? typeof(int?) : typeof(int);
                case "bigint":
                    return isNullable ? typeof(long?) : typeof(long);
                case "float":
                    return isNullable ? typeof(double?) : typeof(double);
                case "real":
                    return isNullable ? typeof(float?) : typeof(float);
                case "bit":
                    return isNullable ? typeof(bool?) : typeof(bool);
                case "uniqueidentifier":
                    return isNullable ? typeof(Guid?) : typeof(Guid);
                case "smallint":
                    return isNullable ? typeof(short?) : typeof(short);
                case "datetimeoffset":
                    return isNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
                case "time":
                    return isNullable ? typeof(TimeSpan?) : typeof(TimeSpan);
                case "sql_variant":
                    return typeof(object);
                case "tinyint":
                    return isNullable ? typeof(byte?) : typeof(byte);
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                case "xml":
                    return typeof(string);
                case "data":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return isNullable ? typeof(DateTime?) : typeof(DateTime);
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return isNullable ? typeof(decimal?) : typeof(decimal);
                case "binary":
                case "timestamp":
                case "varbinary":
                case "image":
                    return typeof(byte[]);
                default:
                    return typeof(object);
            }
        }
    }
}