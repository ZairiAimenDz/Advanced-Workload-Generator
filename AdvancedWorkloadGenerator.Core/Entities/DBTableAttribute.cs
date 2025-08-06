using System.ComponentModel.DataAnnotations;

namespace AdvancedWorkloadGenerator.Core.Entities
{
    // Table Attribute Entity
    public class DBTableAttribute
    {
        public Guid Id { get; set; } = Guid.NewGuid();


        public string AttributeName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;

        public int? MaxLength { get; set; }
        public bool IsNullable { get; set; } = true;
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsForeignKey { get; set; } = false;
        public bool IsUnique { get; set; } = false;
        public bool HasIndex { get; set; } = false;
        public string? IndexName { get; set; } = string.Empty;
        public bool IsUniqueIndex { get; set; } = false;

        public string DefaultValue { get; set; } = string.Empty;
        public int OrdinalPosition { get; set; }

        // Foreign key reference information
        public string? ReferencedTableName { get; set; } = string.Empty;
        public string? ReferencedAttributeName { get; set; } = string.Empty;

        // Literal Foreign Key
        public Guid? ReferencedTableId { get; set; }
        public virtual DBTable? ReferencedTable { get; set; }

        // Table
        public Guid TableId { get; set; }
        public virtual DBTable Table { get; set; }
    }
}
