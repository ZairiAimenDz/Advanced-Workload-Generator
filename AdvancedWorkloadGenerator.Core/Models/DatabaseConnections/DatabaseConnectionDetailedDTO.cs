using AdvancedWorkloadGenerator.Core.Models.DatabaseTables;

namespace AdvancedWorkloadGenerator.Core.Models.DatabaseConnections
{
    public class DatabaseConnectionDetailedDTO : DatabaseConnectionDTO
    {
        public List<DatabaseTableDTO> DatabaseTables { get; set; } = new List<DatabaseTableDTO>();
    }

}
