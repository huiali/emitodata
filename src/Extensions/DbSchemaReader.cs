using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Huiali.ILOData.Models;

namespace Huiali.ILOData.Extensions
{
    public class DbSchemaReader
    {
        private static IEnumerable<Column> GetColumns(string connectionString)
        {
            const string sql = @"
            select 
                SchemaName = schema_name(t.schema_id),
	            TableName = t.name,
	            ColumnName = c.name,
                ColumnType = TYPE_NAME(c.system_type_id),
	            IsPrimaryKey = (select count(*) from
                        sys.indexes as i join sys.index_columns as ic on i.OBJECT_ID = ic.OBJECT_ID and i.index_id = ic.index_id and ic.column_id = c.column_id
                        where i.is_primary_key = 1 and i.object_id = t.object_id),
                IsNullable= c.is_nullable,
                IsIdentity= c.is_identity,
                MaxLength = c.max_length
            from sys.tables t join sys.columns c on t.object_id = c.object_id
            order by t.name , c.column_id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sql,connection);
                List<Column> columns = new List<Column>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new Column
                        {
                            SchemaName = reader[0].ToString(),
                            TableName = reader[1].ToString(),
                            ColumnName = reader[2].ToString(),
                            ColumnType = reader[3].ToString(),
                            IsPrimaryKey = Convert.ToBoolean(reader[4]),
                            IsNullable = Convert.ToBoolean(reader[5].ToString()),
                            IsIdentity = Convert.ToBoolean(reader[6].ToString()),
                            MaxLength = Convert.ToInt32(reader[7].ToString()),
                        });
                    }
                    return columns;
                }
            }
        }

        public static IEnumerable<Table> GetSchemata(string connectionString)
        {
            var columns = GetColumns(connectionString);
            var tables = columns.GroupBy(p => p.TableName).Select(t => new Table
            {
                Name = t.Key,
                Columns = t.AsEnumerable()
            });
            return tables;
        }
    }
}