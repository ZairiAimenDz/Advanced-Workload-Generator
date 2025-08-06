using AdvancedWorkloadGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdvancedWorkloadGenerator.Data.Context
{
    public class GeneratorDbContextFactory : IDesignTimeDbContextFactory<GeneratorDbContext>
    {
        public GeneratorDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GeneratorDbContext>();
            
            // Use SQLite with a temporary connection string for migrations
            optionsBuilder.UseSqlite("Data Source=tempdb.db");
            
            return new GeneratorDbContext(optionsBuilder.Options);
        }
    }
}