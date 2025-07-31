using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.Wrappers;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class DatabaseAnalysisService : IDatabaseAnalysisService
    {
        public Task<Response<bool>> AnalyseDatabase(Guid DatabaseConnectionId)
        {
            throw new NotImplementedException();
        }
    }
}
