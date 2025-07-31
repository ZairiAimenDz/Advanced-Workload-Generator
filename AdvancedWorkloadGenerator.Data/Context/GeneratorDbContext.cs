using AdvancedWorkloadGenerator.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Data.Context
{
    public class GeneratorDbContext : DbContext
    {
       public GeneratorDbContext(DbContextOptions<GeneratorDbContext> options)
            : base(options)
        {
        }

        /*        protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    // Configure your entity mappings here
                    // Example: modelBuilder.Entity<YourEntity>().ToTable("YourTableName");
                }
        */

        public DbSet<DBConnection> Connections { get; set; }
        public DbSet<DBTable> Tables { get; set; }
        public DbSet<DBTableAttribute> Columns { get; set; }
        public DbSet<QueryGenerationRequest> GenerationRequests { get; set; }
    }
}
