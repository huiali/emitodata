using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using Huiali.EmitOData.Models;

namespace Huiali.EmitOData.Extensions
{
    public class DbSchemaReader
    {
        private static IEnumerable<Column> GetColumns(string connectionString)
        {
            const string sql = @"SELECT
    SCHEMA_NAME([t].[schema_id]) AS [SchemaName],
    [t].[name] AS [TableName],
    [c].[name] AS [ColumnName],
    [tp].[name] AS [ColumnType],
	(select count(*) from
                        sys.indexes as i join sys.index_columns as ic on i.OBJECT_ID = ic.OBJECT_ID and i.index_id = ic.index_id and ic.column_id = c.column_id
                        where i.is_primary_key = 1 and i.object_id = t.object_id) as IsPrimaryKey,
	[c].[is_nullable] AS [IsNullable],
	[c].[is_identity] AS [IsIdentity],
	CAST([c].[max_length] AS int) AS [MaxLength],
    [c].[column_id] AS [ordinal],
    SCHEMA_NAME([tp].[schema_id]) AS [type_schema],
    CAST([c].[precision] AS int) AS [precision],
    CAST([c].[scale] AS int) AS [scale],
    OBJECT_DEFINITION([c].[default_object_id]) AS [default_sql],
    [cc].[definition] AS [computed_sql]
FROM [sys].[columns] AS [c]
JOIN [sys].[tables] AS [t] ON [c].[object_id] = [t].[object_id]
JOIN [sys].[types] AS [tp] ON [c].[user_type_id] = [tp].[user_type_id]
LEFT JOIN [sys].[computed_columns] AS [cc] ON [c].[object_id] = [cc].[object_id] AND [c].[column_id] = [cc].[column_id]
WHERE [t].[is_ms_shipped] = 0
AND NOT EXISTS (SELECT *
    FROM [sys].[extended_properties] AS [ep]
    WHERE [ep].[major_id] = [t].[object_id]
        AND [ep].[minor_id] = 0
        AND [ep].[class] = 1
        AND [ep].[name] = N'microsoft_database_tools_support'
    )
AND [t].[name] <> '__EFMigrationsHistory'
AND [t].[temporal_type] <> 1 AND [c].[is_hidden] = 0
ORDER BY [SchemaName], [TableName], [c].[column_id]";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
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
                            ColumnType = ClrTypeConvert.GetRuntimeType(reader[3].ToString(), Convert.ToBoolean(reader[5].ToString())),
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
            var tables = columns.GroupBy(p => $"{p.SchemaName}`{p.TableName}").Select(t => new Table
            {
                Schema = t.Key.Split('`')[0],
                Name = t.Key.Split('`')[1],
                Columns = t.AsEnumerable()
            });
            return tables;
        }
    }
}