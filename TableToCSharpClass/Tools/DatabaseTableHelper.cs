using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace AdvExample1
{
    /// <summary>Used to work with database tables.</summary>
    public class DatabaseTableHelper 
    {
        private readonly SqlConnection _connection;

        /// <summary>Constructor</summary>
        public DatabaseTableHelper(SqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>Produces a C# class based on table field definitions.</summary>
        /// <param name="fieldList">List of fields</param>
        /// <param name="className">Name of the new class</param>
        /// <param name="theNamespace">Namespace fo the new class</param>
        /// <returns>String representation of a new class</returns>
        public string ConvertFieldsToClass(List<DatabaseTableField> fieldList, string className, string theNamespace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {theNamespace}");
            sb.AppendLine("{");

            // Class start
            sb.AppendLine($"    public class {className}");
            sb.AppendLine("    {");
            foreach (var field in fieldList)
            {
                string dataType = field.DataTypeAsCSharpType().HelpTypeToString();
                sb.AppendFormat("        public {0} {1} {{ get; set; }}", dataType, field.ColumnName);
                sb.AppendLine();
            }
            sb.AppendLine("    }");
            // Class end

            sb.AppendLine("}");  // close namespace

            return sb.ToString();
        }

        /// <summary>Loads field definitions from the database table.</summary>
        /// <param name="tableName">Name of a table.</param>
        /// <returns></returns>
        public List<DatabaseTableField> LoadFields(string tableName)
        {
            var results = new List<DatabaseTableField>();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT c.name 'ColumnName', t.Name 'DataType', c.max_length 'MaxLength',");
            sb.AppendLine("c.precision,  c.scale,  c.is_nullable, ISNULL(i.is_primary_key, 0) 'PrimaryKey'");
            sb.AppendLine("FROM sys.columns c");
            sb.AppendLine("INNER JOIN  sys.types t ON c.user_type_id = t.user_type_id");
            sb.AppendLine("LEFT OUTER JOIN  sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id");
            sb.AppendLine("LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id");
            sb.AppendFormat("WHERE c.object_id = OBJECT_ID('{0}')", tableName);

            using (SqlCommand command = new SqlCommand(sb.ToString(), _connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var newField = new DatabaseTableField
                        {
                            ColumnName = reader["ColumnName"].ToString(),
                            DataType = reader["DataType"].ToString(),
                            MaxLength = int.Parse(reader["MaxLength"].ToString()),
                            Precision = int.Parse(reader["precision"].ToString()),
                            Scale = int.Parse(reader["scale"].ToString()),
                            IsNullable = bool.Parse(reader["is_nullable"].ToString()),
                            IsPrimaryKey = bool.Parse(reader["PrimaryKey"].ToString())
                        };

                        results.Add(newField);
                    }
                }
            }
            
            return results;
        }       
        
        /// <summary>Loads a list of user tables from the database.</summary>
        public List<DatabaseTable> LoadTableNames()
        {
            var results = new List<DatabaseTable>();

            const string query = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";

            using (SqlCommand command = new SqlCommand(query, _connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var newTable = new DatabaseTable
                        {
                            Schema = reader["TABLE_SCHEMA"].ToString(),
                            TableName = reader["TABLE_NAME"].ToString()                           
                        };

                        results.Add(newTable);
                    }
                }
            }

            return results;

        }
        
        /// <summary>Truncates a list of tables.</summary>
        /// <param name="tableList">List of tables</param>
        public void TruncateTables(List<string> tableList)
        {
            foreach (var tableName in tableList)
            {
                TruncateTable(tableName);
            }
        }

        /// <summary>Truncates one table.</summary>
        /// <param name="tableName">Table to trunate</param>
        public void TruncateTable(string tableName)
        {
            // Delete all from the destination table.
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = _connection;
                cmd.CommandText = $"Truncate table {tableName}";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
