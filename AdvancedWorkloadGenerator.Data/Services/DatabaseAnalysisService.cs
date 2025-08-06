using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.Wrappers;
using AdvancedWorkloadGenerator.Core.Models.DatabaseTables;
using AdvancedWorkloadGenerator.Core.Entities;
using AdvancedWorkloadGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Npgsql;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class DatabaseAnalysisService(GeneratorDbContext context, ILogger<DatabaseAnalysisService> logger) : IDatabaseAnalysisService
    {
        public async Task<Response<bool>> AnalyseDatabase(Guid DatabaseConnectionId)
        {
            try
            {
                var connection = await context.Connections
                    .Include(c => c.DatabaseTables)
                    .FirstOrDefaultAsync(c => c.Id == DatabaseConnectionId);

                if (connection == null)
                {
                    return Response<bool>.Failure("Database connection not found");
                }

                // Clear existing analysis data
                if (connection.DatabaseTables.Any())
                {
                    context.Tables.RemoveRange(connection.DatabaseTables);
                }

                // Perform database schema analysis
                var tables = await AnalyzeDatabaseSchemaAsync(connection);
                
                // Save the analyzed tables
                foreach (var table in tables)
                {
                    var dbTable = new DBTable
                    {
                        Id = Guid.NewGuid(),
                        TableName = table.TableName,
                        Schema = table.SchemaName ?? "public",
                        DatabaseConnectionId = connection.Id,
                        DatabaseConnection = connection
                    };
                    
                    // Add table attributes (columns)
                    if (table.Columns != null)
                    {
                        int ordinalPosition = 1;
                        foreach (var column in table.Columns)
                        {
                            var attribute = new DBTableAttribute
                            {
                                Id = Guid.NewGuid(),
                                AttributeName = column.ColumnName,
                                DataType = column.DataType,
                                IsNullable = column.IsNullable,
                                IsPrimaryKey = column.IsPrimaryKey,
                                IsForeignKey = column.IsForeignKey,
                                MaxLength = column.MaxLength,
                                ReferencedTableName = column.ReferencedTable,
                                ReferencedAttributeName = column.ReferencedColumn,
                                HasIndex = column.HasIndex,
                                IndexName = column.IndexName,
                                IsUniqueIndex = column.IsUniqueIndex,
                                OrdinalPosition = ordinalPosition++,
                                TableId = dbTable.Id,
                                Table = dbTable
                            };
                            dbTable.TableAttributes.Add(attribute);
                        }
                    }
                    
                    context.Tables.Add(dbTable);
                }

                // Update connection analysis status
                connection.IsAnalyzed = true;
                connection.LastAnalyzedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return Response<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure($"Analysis failed: {ex.Message}");
            }
        }

        private async Task<List<DatabaseTableDTO>> AnalyzeDatabaseSchemaAsync(DBConnection connection)
        {
            var tables = new List<DatabaseTableDTO>();

            try
            {
                using var dbConnection = CreateDbConnection(connection);
                await dbConnection.OpenAsync();

                var tableNames = await GetTableNamesAsync(dbConnection, connection.DatabaseType);
                
                foreach (var tableName in tableNames)
                {
                    var columns = await GetTableColumnsAsync(dbConnection, connection.DatabaseType, tableName);
                    
                    tables.Add(new DatabaseTableDTO
                    {
                        TableName = tableName,
                        SchemaName = "public", // Default schema, could be enhanced
                        Columns = columns
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to analyze database schema: {ex.Message}", ex);
            }

            return tables;
        }

        private DbConnection CreateDbConnection(DBConnection connection)
        {
            return connection.DatabaseType switch
            {
                Core.Enums.DatabaseType.PostgreSQL => new NpgsqlConnection(connection.ConnectionString),
                //Core.Enums.DatabaseType.SqlServer => new SqlConnection(connection.ConnectionString),
                //Core.Enums.DatabaseType.MySQL => new MySqlConnection(connection.ConnectionString),
                _ => throw new NotSupportedException($"Database type {connection.DatabaseType} is not supported")
            };
        }

        private async Task<List<string>> GetTableNamesAsync(DbConnection connection, Core.Enums.DatabaseType dbType)
        {
            var tables = new List<string>();
            
            var query = dbType switch
            {
                Core.Enums.DatabaseType.PostgreSQL => 
                    "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'",
                //Core.Enums.DatabaseType.SqlServer => 
                //    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                //Core.Enums.DatabaseType.MySQL => 
                //    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE'",
                _ => throw new NotSupportedException($"Database type {dbType} is not supported")
            };

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        private async Task<List<DatabaseColumnDTO>> GetTableColumnsAsync(DbConnection connection, Core.Enums.DatabaseType dbType, string tableName)
        {
            if (dbType == Core.Enums.DatabaseType.PostgreSQL)
            {
                return await GetTableColumnsEnhancedAsync(connection, tableName);
            }

            // Fallback to basic implementation for other database types
            var columns = new List<DatabaseColumnDTO>();
            
            var query = dbType switch
            {
                Core.Enums.DatabaseType.PostgreSQL => @"
                    SELECT 
                        c.column_name,
                        c.data_type,
                        c.is_nullable = 'YES' as is_nullable,
                        c.character_maximum_length,
                        CASE WHEN c.column_default LIKE 'nextval%' THEN true ELSE false END as is_auto_increment,
                        CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key
                    FROM information_schema.columns c
                    LEFT JOIN (
                        SELECT ku.column_name
                        FROM information_schema.table_constraints tc
                        JOIN information_schema.key_column_usage ku ON tc.constraint_name = ku.constraint_name
                        WHERE tc.table_name = @tableName AND tc.constraint_type = 'PRIMARY KEY'
                    ) pk ON c.column_name = pk.column_name
                    WHERE c.table_name = @tableName
                    ORDER BY c.ordinal_position",
                
                _ => throw new NotSupportedException($"Database type {dbType} is not supported")
            };

            using var command = connection.CreateCommand();
            command.CommandText = query;
            
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(new DatabaseColumnDTO
                {
                    ColumnName = reader.GetString(0),  // column_name
                    DataType = reader.GetString(1),    // data_type
                    IsNullable = reader.GetBoolean(2), // is_nullable
                    MaxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3), // character_maximum_length
                    IsAutoIncrement = reader.GetBoolean(4), // is_auto_increment
                    IsPrimaryKey = reader.GetBoolean(5)     // is_primary_key
                });
            }

            return columns;
        }

        /// <summary>
        /// Enhanced PostgreSQL column analysis with foreign keys and indexes
        /// </summary>
        private async Task<List<DatabaseColumnDTO>> GetTableColumnsEnhancedAsync(DbConnection connection, string tableName)
        {
            var columns = new List<DatabaseColumnDTO>();
            
                            logger.LogInformation("Analyzing table: {TableName}", tableName);
            
            // Get basic column information
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
                    SELECT column_name, data_type, is_nullable, ordinal_position, character_maximum_length,
                           CASE WHEN column_default LIKE 'nextval%' THEN true ELSE false END as is_auto_increment
                    FROM information_schema.columns 
                    WHERE table_schema = 'public' 
                    AND table_name = '{tableName}' 
                    ORDER BY ordinal_position";
                
                using var reader = await command.ExecuteReaderAsync();
                logger.LogDebug("Retrieving columns for table {TableName}", tableName);
                while (await reader.ReadAsync())
                {
                    var colName = reader.GetString(0);
                    var dataType = reader.GetString(1);
                    var isNullable = reader.GetString(2) == "YES";
                    var position = reader.GetInt32(3);
                    var maxLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4);
                    var isAutoIncrement = reader.GetBoolean(5);
                    
                    logger.LogDebug("Found column: {ColumnName} ({DataType}) nullable: {IsNullable} position: {Position}", colName, dataType, isNullable, position);
                    
                    columns.Add(new DatabaseColumnDTO
                    {
                        ColumnName = colName,
                        DataType = dataType,
                        IsNullable = isNullable,
                        MaxLength = maxLength,
                        IsAutoIncrement = isAutoIncrement
                    });
                }
            }
            
            // Get primary keys
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
                    SELECT column_name
                    FROM information_schema.key_column_usage 
                    WHERE table_schema = 'public' 
                    AND table_name = '{tableName}' 
                    AND constraint_name LIKE '%_pkey'";
                
                using var reader = await command.ExecuteReaderAsync();
                var primaryKeys = new List<string>();
                while (await reader.ReadAsync())
                {
                    primaryKeys.Add(reader.GetString(0));
                }
                
                logger.LogDebug("Primary keys found: {PrimaryKeys}", string.Join(", ", primaryKeys));
                
                foreach (var col in columns)
                {
                    col.IsPrimaryKey = primaryKeys.Contains(col.ColumnName);
                }
            }
            
            // Get foreign keys with referenced tables and columns
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
                    SELECT 
                        kcu.column_name,
                        ccu.table_name as referenced_table,
                        ccu.column_name as referenced_column
                    FROM information_schema.key_column_usage kcu
                    JOIN information_schema.constraint_column_usage ccu 
                        ON kcu.constraint_name = ccu.constraint_name
                    WHERE kcu.table_schema = 'public' 
                    AND kcu.table_name = '{tableName}'
                    AND kcu.constraint_name LIKE '%_fkey'";
                
                using var reader = await command.ExecuteReaderAsync();
                logger.LogDebug("Retrieving foreign keys for table {TableName}", tableName);
                while (await reader.ReadAsync())
                {
                    var colName = reader.GetString(0);
                    var refTable = reader.GetString(1);
                    var refColumn = reader.GetString(2);
                    logger.LogDebug("Found foreign key: {ColumnName} -> {ReferencedTable}.{ReferencedColumn}", colName, refTable, refColumn);
                    
                    var col = columns.FirstOrDefault(c => c.ColumnName == colName);
                    if (col != null)
                    {
                        col.IsForeignKey = true;
                        col.ReferencedTable = refTable;
                        col.ReferencedColumn = refColumn;
                    }
                }
            }
            
            // Get indexes
            var indexes = await GetTableIndexesAsync(connection, tableName);
            logger.LogDebug("Found {IndexCount} indexes for table {TableName}", indexes.Count, tableName);
            
            foreach (var index in indexes)
            {
                var col = columns.FirstOrDefault(c => c.ColumnName == index.ColumnName);
                if (col != null)
                {
                    col.HasIndex = true;
                    col.IndexName = index.IndexName;
                    col.IsUniqueIndex = index.IsUnique;
                    col.IndexNames.Add(index.IndexName);
                    logger.LogDebug("Found index: {IndexName} on column {ColumnName} (unique: {IsUnique})", index.IndexName, index.ColumnName, index.IsUnique);
                }
            }

            return columns;
        }

        /// <summary>
        /// Get table indexes for PostgreSQL
        /// </summary>
        private async Task<List<IndexInfo>> GetTableIndexesAsync(DbConnection connection, string tableName)
        {
            var indexes = new List<IndexInfo>();
            
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT 
                    i.relname as index_name,
                    a.attname as column_name,
                    ix.indisunique as is_unique
                FROM pg_class t
                JOIN pg_index ix ON t.oid = ix.indrelid
                JOIN pg_class i ON ix.indexrelid = i.oid
                JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = ANY(ix.indkey)
                JOIN pg_namespace n ON t.relnamespace = n.oid
                WHERE n.nspname = 'public'
                AND t.relname = '{tableName}'
                AND i.relname NOT LIKE '%_pkey'
                ORDER BY i.relname, a.attnum";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(new IndexInfo
                {
                    Name = reader.GetString(0),
                    ColumnName = reader.GetString(1),
                    IsUnique = reader.GetBoolean(2)
                });
            }

            return indexes;
        }
    }
}
