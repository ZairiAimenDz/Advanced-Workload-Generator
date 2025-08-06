using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Models.DatabaseTables
{
    public class DatabaseTableDTO
    {
        public Guid Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? SchemaName { get; set; } = "public";
        public string Schema { get; set; } = string.Empty;
        public int RecordCount { get; set; } = 0;
        public Guid DatabaseConnectionId { get; set; }
        public List<DatabaseColumnDTO>? Columns { get; set; }
    }
}
