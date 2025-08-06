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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DBConnection entity
            modelBuilder.Entity<DBConnection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ConnectionString).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Host).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DatabaseType).IsRequired();
                entity.Property(e => e.Port).IsRequired();
                entity.Property(e => e.IsAnalyzed).HasDefaultValue(false);
                entity.Property(e => e.LastAnalyzedAt).IsRequired(false);

                // Configure one-to-many relationship with DBTable
                entity.HasMany(e => e.DatabaseTables)
                      .WithOne(t => t.DatabaseConnection)
                      .HasForeignKey(t => t.DatabaseConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DBTable entity
            modelBuilder.Entity<DBTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TableName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Schema).IsRequired().HasMaxLength(255);
                entity.Property(e => e.RecordCount).HasDefaultValue(0);
                entity.Property(e => e.DatabaseConnectionId).IsRequired();

                // Configure one-to-many relationship with DBTableAttribute
                entity.HasMany(e => e.TableAttributes)
                      .WithOne(a => a.Table)
                      .HasForeignKey(a => a.TableId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure foreign key to DBConnection
                entity.HasOne(e => e.DatabaseConnection)
                      .WithMany(c => c.DatabaseTables)
                      .HasForeignKey(e => e.DatabaseConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DBTableAttribute entity
            modelBuilder.Entity<DBTableAttribute>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AttributeName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MaxLength).IsRequired(false);
                entity.Property(e => e.IsNullable).HasDefaultValue(true);
                entity.Property(e => e.IsPrimaryKey).HasDefaultValue(false);
                entity.Property(e => e.IsForeignKey).HasDefaultValue(false);
                entity.Property(e => e.IsUnique).HasDefaultValue(false);
                entity.Property(e => e.HasIndex).HasDefaultValue(false);
                entity.Property(e => e.IndexName).IsRequired(false).HasMaxLength(255);
                entity.Property(e => e.IsUniqueIndex).HasDefaultValue(false);
                entity.Property(e => e.DefaultValue).HasMaxLength(500);
                entity.Property(e => e.OrdinalPosition).IsRequired();
                entity.Property(e => e.ReferencedTableName).IsRequired(false).HasMaxLength(255);
                entity.Property(e => e.ReferencedAttributeName).IsRequired(false).HasMaxLength(255);
                entity.Property(e => e.ReferencedTableId).IsRequired(false);
                entity.Property(e => e.TableId).IsRequired();

                // Configure foreign key to parent table (required)
                entity.HasOne(e => e.Table)
                      .WithMany(t => t.TableAttributes)
                      .HasForeignKey(e => e.TableId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure optional foreign key to referenced table (nullable)
                entity.HasOne(e => e.ReferencedTable)
                      .WithMany()
                      .HasForeignKey(e => e.ReferencedTableId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            // Configure QueryGenerationRequest entity
            modelBuilder.Entity<QueryGenerationRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RequestName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.GeneratedQueries).HasDefaultValue(0);
                entity.Property(e => e.ExecutedQueries).HasDefaultValue(0);
                entity.Property(e => e.GenerationStatus).IsRequired();
                entity.Property(e => e.ExecutionStatus).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.StartedExecutionAt).IsRequired(false);
                entity.Property(e => e.FinishedExecutionAt).IsRequired(false);
                entity.Property(e => e.StartedGenerationAt).IsRequired(false);
                entity.Property(e => e.FinishedGenerationAt).IsRequired(false);
                entity.Property(e => e.ErrorMessage).IsRequired(false).HasMaxLength(2000);
                entity.Property(e => e.ResultFilePath).IsRequired(false).HasMaxLength(500);
                entity.Property(e => e.ResultCSVPath).IsRequired(false).HasMaxLength(500);
                entity.Property(e => e.DatabaseConnectionId).IsRequired();

                // Configure foreign key to DBConnection
                entity.HasOne(e => e.DatabaseConnection)
                      .WithMany()
                      .HasForeignKey(e => e.DatabaseConnectionId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.ComplexProperty(e => e.Parameters);
            });
        }

        public DbSet<DBConnection> Connections { get; set; }
        public DbSet<DBTable> Tables { get; set; }
        public DbSet<DBTableAttribute> Columns { get; set; }
        public DbSet<QueryGenerationRequest> GenerationRequests { get; set; }
    }
}
