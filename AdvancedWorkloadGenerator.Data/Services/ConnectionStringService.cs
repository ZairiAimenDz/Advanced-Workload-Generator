using AdvancedWorkloadGenerator.Core.Entities;
using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Models.Wrappers;
using AdvancedWorkloadGenerator.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class ConnectionStringService(GeneratorDbContext context, ILogger<ConnectionStringService> logger) : IConnectionStringService
    {
        public async Task<Response<DatabaseConnectionDetailedDTO>> CreateConnectionString(DatabaseConnectionCreateDTO connectionString)
        {
            try
            {
                var dbConnection = new DBConnection
                {
                    Id = Guid.NewGuid(),
                    DatabaseType = connectionString.DatabaseType,
                    Name = connectionString.Name,
                    ConnectionString = connectionString.ConnectionString,
                    Host = connectionString.Host,
                    Port = connectionString.Port,
                    DatabaseName = connectionString.DatabaseName,
                    Username = connectionString.Username,
                    Password = connectionString.Password,
                    IsAnalyzed = false,
                    LastAnalyzedAt = null
                };

                context.Connections.Add(dbConnection);
                await context.SaveChangesAsync();

                var result = new DatabaseConnectionDetailedDTO
                {
                    Id = dbConnection.Id,
                    DatabaseType = dbConnection.DatabaseType,
                    Name = dbConnection.Name,
                    ConnectionString = dbConnection.ConnectionString,
                    Host = dbConnection.Host,
                    Port = dbConnection.Port,
                    DatabaseName = dbConnection.DatabaseName,
                    Username = dbConnection.Username,
                    Password = dbConnection.Password,
                    IsAnalyzed = dbConnection.IsAnalyzed,
                    LastAnalyzedAt = dbConnection.LastAnalyzedAt,
                    DatabaseTables = new List<Core.Models.DatabaseTables.DatabaseTableDTO>()
                };

                return Response<DatabaseConnectionDetailedDTO>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating connection string for database type {DatabaseType}", connectionString.DatabaseType);
                return Response<DatabaseConnectionDetailedDTO>.Failure($"Error creating connection: {ex.Message}");
            }
        }

        public async Task<Response<bool>> DeleteConnectionString(Guid id)
        {
            try
            {
                var connection = await context.Connections.FindAsync(id);
                if (connection == null)
                {
                    return Response<bool>.Failure("Connection not found");
                }

                context.Connections.Remove(connection);
                await context.SaveChangesAsync();

                return Response<bool>.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting connection with ID {ConnectionId}", id);
                return Response<bool>.Failure($"Error deleting connection: {ex.Message}");
            }
        }

        public async Task<Response<List<DatabaseConnectionDTO>>> GetAllConnectionStrings()
        {
            try
            {
                var connections = await context.Connections
                    .Select(c => new DatabaseConnectionDTO
                    {
                        Id = c.Id,
                        DatabaseType = c.DatabaseType,
                        Name = c.Name,
                        ConnectionString = c.ConnectionString,
                        Host = c.Host,
                        Port = c.Port,
                        DatabaseName = c.DatabaseName,
                        Username = c.Username,
                        Password = c.Password,
                        IsAnalyzed = c.IsAnalyzed,
                        LastAnalyzedAt = c.LastAnalyzedAt
                    })
                    .ToListAsync();

                return Response<List<DatabaseConnectionDTO>>.Success(connections);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all connections");
                return Response<List<DatabaseConnectionDTO>>.Failure($"Error retrieving connections: {ex.Message}");
            }
        }

        public async Task<Response<DatabaseConnectionDetailedDTO>> GetConnectionStringById(Guid id)
        {
            try
            {
                var connection = await context.Connections
                    .Include(c => c.DatabaseTables)
                        .ThenInclude(t => t.TableAttributes)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (connection == null)
                {
                    return Response<DatabaseConnectionDetailedDTO>.Failure("Connection not found");
                }

                var result = new DatabaseConnectionDetailedDTO
                {
                    Id = connection.Id,
                    DatabaseType = connection.DatabaseType,
                    Name = connection.Name,
                    ConnectionString = connection.ConnectionString,
                    Host = connection.Host,
                    Port = connection.Port,
                    DatabaseName = connection.DatabaseName,
                    Username = connection.Username,
                    Password = connection.Password,
                    IsAnalyzed = connection.IsAnalyzed,
                    LastAnalyzedAt = connection.LastAnalyzedAt,
                    DatabaseTables = connection.DatabaseTables.Select(t => new Core.Models.DatabaseTables.DatabaseTableDTO
                    {
                        Id = t.Id,
                        TableName = t.TableName,
                        Schema = t.Schema,
                        SchemaName = t.Schema,
                        RecordCount = t.RecordCount,
                        DatabaseConnectionId = t.DatabaseConnectionId,
                        Columns = t.TableAttributes.Select(attr => new Core.Models.DatabaseTables.DatabaseColumnDTO
                        {
                            ColumnName = attr.AttributeName,
                            DataType = attr.DataType,
                            IsNullable = attr.IsNullable,
                            IsPrimaryKey = attr.IsPrimaryKey,
                            IsAutoIncrement = false, // Will need to enhance this if needed
                            MaxLength = attr.MaxLength,
                            IsForeignKey = attr.IsForeignKey,
                            ReferencedTable = attr.ReferencedTableName,
                            ReferencedColumn = attr.ReferencedAttributeName,
                            HasIndex = attr.HasIndex,
                            IndexName = attr.IndexName,
                            IsUniqueIndex = attr.IsUniqueIndex
                        }).ToList()
                    }).ToList()
                };

                return Response<DatabaseConnectionDetailedDTO>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving connection with ID {ConnectionId}", id);
                return Response<DatabaseConnectionDetailedDTO>.Failure($"Error retrieving connection: {ex.Message}");
            }
        }

        public async Task<Response<DatabaseConnectionDTO>> UpdateConnectionString(Guid id, DatabaseConnectionCreateDTO connectionString)
        {
            try
            {
                var existingConnection = await context.Connections.FindAsync(id);
                if (existingConnection == null)
                {
                    return Response<DatabaseConnectionDTO>.Failure("Connection not found");
                }

                existingConnection.DatabaseType = connectionString.DatabaseType;
                existingConnection.Name = connectionString.Name;
                existingConnection.ConnectionString = connectionString.ConnectionString;
                existingConnection.Host = connectionString.Host;
                existingConnection.Port = connectionString.Port;
                existingConnection.DatabaseName = connectionString.DatabaseName;
                existingConnection.Username = connectionString.Username;
                existingConnection.Password = connectionString.Password;

                context.Connections.Update(existingConnection);
                await context.SaveChangesAsync();

                var result = new DatabaseConnectionDTO
                {
                    Id = existingConnection.Id,
                    DatabaseType = existingConnection.DatabaseType,
                    Name = existingConnection.Name,
                    ConnectionString = existingConnection.ConnectionString,
                    Host = existingConnection.Host,
                    Port = existingConnection.Port,
                    DatabaseName = existingConnection.DatabaseName,
                    Username = existingConnection.Username,
                    Password = existingConnection.Password,
                    IsAnalyzed = existingConnection.IsAnalyzed,
                    LastAnalyzedAt = existingConnection.LastAnalyzedAt
                };

                return Response<DatabaseConnectionDTO>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating connection with ID {ConnectionId}", id);
                return Response<DatabaseConnectionDTO>.Failure($"Error updating connection: {ex.Message}");
            }
        }
    }
}
