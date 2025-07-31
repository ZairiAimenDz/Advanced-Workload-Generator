using AdvancedWorkloadGenerator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Models.DatabaseConnections
{
    public class DatabaseConnectionCreateDTO
    {
        public DatabaseType DatabaseType { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432; // Default PostgreSQL port
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
