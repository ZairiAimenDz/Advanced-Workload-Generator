using System;
using System.Collections.Generic;

namespace AdvancedWorkloadGenerator.Core.Models.DatabaseTables
{
    public class DatabaseColumnDTO
    {
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public int? MaxLength { get; set; }
        public bool IsForeignKey { get; set; }
        public string? ReferencedTable { get; set; }
        public string? ReferencedColumn { get; set; }
        
        // Index information
        public bool HasIndex { get; set; }
        public string? IndexName { get; set; }
        public bool IsUniqueIndex { get; set; }
        public List<string> IndexNames { get; set; } = new List<string>();
        
        // Enhanced display properties
        public string DisplayText
        {
            get
            {
                var result = $"{ColumnName} ({DataType})";
                if (!IsNullable) result += " NOT NULL";
                if (IsPrimaryKey) result += " [PK]";
                if (IsForeignKey) result += $" [FK → {ReferencedTable}]";
                if (HasIndex) result += " [IDX]";
                return result;
            }
        }
    }
    
    // Helper classes for enhanced analysis
    public class ColumnInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string? ForeignKeyTable { get; set; }
        public string? ForeignKeyColumn { get; set; }
        public bool HasIndex { get; set; }
        public string? IndexName { get; set; }
        public bool IsUniqueIndex { get; set; }
        public int? MaxLength { get; set; }
        public bool IsAutoIncrement { get; set; }
    }

    public class IndexInfo
    {
        public string Name { get; set; } = "";
        public string ColumnName { get; set; } = "";
        public bool IsUnique { get; set; }
        public string IndexName => Name;
    }

    public class TableDataResult
    {
        public List<string> ColumnNames { get; set; } = new List<string>();
        public List<List<object?>> Rows { get; set; } = new List<List<object?>>();
        public int TotalRowsReturned { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}