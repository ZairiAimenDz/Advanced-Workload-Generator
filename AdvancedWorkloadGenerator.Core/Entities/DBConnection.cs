using AdvancedWorkloadGenerator.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Entities
{
    public class DBConnection
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DatabaseType DatabaseType { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432; // Default PostgreSQL port
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        //TODO: Consider using a secure way to store passwords if needed

        public DateTime? LastAnalyzedAt { get; set; }
        public bool IsAnalyzed { get; set; } = false;

        public virtual ICollection<DBTable> DatabaseTables { get; set; } = new List<DBTable>();
    }
}
