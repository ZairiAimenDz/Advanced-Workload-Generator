namespace AdvancedWorkloadGenerator.Core.Models.DatabaseConnections
{
    public class DatabaseConnectionDTO : DatabaseConnectionUpdateDTO
    {
        public Guid Id { get; set; }
        public DateTime? LastAnalyzedAt { get; set; }
        public bool IsAnalyzed { get; set; } = false;
    }

}
