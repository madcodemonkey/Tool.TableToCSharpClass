using System;

namespace AdvExample1
{

    public class DatabaseTableField
    {
        /// <summary>Name of the column</summary>
        public string ColumnName { get; set; }

        /// <summary>The sql server data type</summary>
        public string DataType { get; set; }

        /// <summary>This is (for almost all datatypes) the maximum length of the column in bytes.  So a column that is declared to be varchar(10) will show 10 in that column.  
        /// A column that is nvarchar(10) will show 20 in that column since each nvarchar character takes 2 bytes.  If the value of the max_length is -1 then the column 
        /// was declared as varchar(max), nvarchar(max), or varbinary(max).</summary>
        /// <remarks>https://social.msdn.microsoft.com/Forums/sqlserver/en-US/be4ad98b-473a-4183-a541-71c3fd79d958/length-of-nvarchar-column-in-system-tables?forum=transactsql</remarks>
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }

        /// <summary>Indicates if the fields in nullable in the database table</summary>
        public bool IsNullable { get; set; }

        /// <summary>Indicates if the field is the primary key in the database table</summary>
        public bool IsPrimaryKey { get; set; }

        public override string ToString()
        {
            return string.Format("ColumnName: {0}  DataType: {1} C#: {2}  IsPrimaryKey: {3}",
                ColumnName,
                string.IsNullOrWhiteSpace(DataType) ? "Unknown" : DataTypeAsString(),
                string.IsNullOrWhiteSpace(DataType) ? "Unknown" : DataTypeAsCSharpType(DataType).HelpTypeToString(),
                IsPrimaryKey);
        }


        public Type DataTypeAsCSharpType()
        {
            return DataTypeAsCSharpType(DataType);
        }
        /// <summary>Converts SQL Server data type to C# type</summary>
        /// <remarks>https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings</remarks>
        public Type DataTypeAsCSharpType(string dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
                throw new ArgumentException("No data type specified!");

            switch (dataType.ToLower())
            {
                case "bigint":
                    return IsNullable ? typeof(Int64?) : typeof(Int64);
                case "binary":
                case "image":
                case "timestamp":
                case "varbinary":
                    return typeof(byte[]);
                case "bit":
                    return IsNullable ? typeof(bool?) : typeof(bool);
                case "char":
                    return MaxLength == 1 ? IsNullable ? typeof(char?) : typeof(char) : typeof(string);
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return IsNullable ? typeof(DateTime?) : typeof(DateTime);
                case "datetimeoffset":
                    return IsNullable ? typeof(DateTimeOffset?) : typeof(DateTimeOffset);
                case "decimal":
                case "numeric":
                case "smallmoney":
                    return IsNullable ? typeof(decimal?) : typeof(decimal);
                case "float":
                    return IsNullable ? typeof(double?) : typeof(double);
                case "int":
                    return IsNullable ? typeof(int?) : typeof(int);
                case "money":
                    return IsNullable ? typeof(decimal?) : typeof(decimal);
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return typeof(string);  // could be char[] too
                case "real":
                    return IsNullable ? typeof(Single?) : typeof(Single);  
                case "rowversion":
                    return typeof(byte[]);
                case "smallint":
                    return IsNullable ? typeof(Int16?) : typeof(Int16);
                case "time":
                    return IsNullable ? typeof(TimeSpan?) : typeof(TimeSpan);
                case "tinyint":
                    return IsNullable ? typeof(byte?) : typeof(byte);
                case "uniqueidentifier":
                    return IsNullable ? typeof(Guid?) : typeof(Guid); 
                default:
                    throw new ArgumentException($"Do not know how to covert the {dataType} type to a C# type.");
            }
        }
        
        /// <summary>Shows the data type as it would appear in sql server management studio with type and parenthesis and
        /// the words null or not null</summary>
        public string DataTypeAsString()
        {
            var result = DataType.ToLower();

            switch (result)
            {
                case "binary":
                case "char":
                case "varbinary":
                    result += string.Format("({0})", MaxLength >= 0 ? MaxLength.ToString() : "max");
                    break;
                case "nchar":
                case "nvarchar":
                case "varchar":
                    result += string.Format("({0})", MaxLength >= 0 ? (MaxLength/2).ToString() : "max");
                    break;
                case "decimal":
                case "numeric":
                    result += string.Format("({0}, {1})", Precision, Scale);
                    break;
            }

            if (IsNullable)
                result += " NULL";
            else result += " NOT NULL";

            return result;
        }
    }
}
