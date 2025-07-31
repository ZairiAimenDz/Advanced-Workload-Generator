namespace AdvancedWorkloadGenerator.Core.Entities
{
    // Database Table Entity
    public class DBTable
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string TableName { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;

        public int RecordCount { get; set; } = 0;

        // Foreign key
        public Guid DatabaseConnectionId { get; set; }
        public virtual DBConnection DatabaseConnection { get; set; }

        // Attributes
        public virtual ICollection<DBTableAttribute> TableAttributes { get; set; } = new List<DBTableAttribute>();
    }
}
