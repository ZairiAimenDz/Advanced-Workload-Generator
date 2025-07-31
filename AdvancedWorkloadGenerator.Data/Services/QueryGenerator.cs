using AdvancedWorkloadGenerator.Core.Interfaces;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class QueryGenerator : IQueryGenerator
    {
        public Task GenerateQueriesAsync(Guid QueryGenRequest)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateSingleQueryAsync(Guid DatabaseConnectionID)
        {
            throw new NotImplementedException();
        }
    }
}
