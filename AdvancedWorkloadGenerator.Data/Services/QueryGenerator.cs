using AdvancedWorkloadGenerator.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class QueryGenerator(ILogger<QueryGenerator> logger) : IQueryGenerator
    {
        public Task GenerateQueriesAsync(Guid QueryGenRequest)
        {
            logger.LogWarning("GenerateQueriesAsync not yet implemented for request {QueryGenRequest}", QueryGenRequest);
            throw new NotImplementedException();
        }

        public Task<string> GenerateSingleQueryAsync(Guid DatabaseConnectionID)
        {
            logger.LogWarning("GenerateSingleQueryAsync not yet implemented for connection {DatabaseConnectionID}", DatabaseConnectionID);
            throw new NotImplementedException();
        }
    }
}
