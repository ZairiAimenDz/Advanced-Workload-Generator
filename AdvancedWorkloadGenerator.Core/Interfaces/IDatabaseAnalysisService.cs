using AdvancedWorkloadGenerator.Core.Models.Wrappers;

namespace AdvancedWorkloadGenerator.Core.Interfaces
{
    public interface IDatabaseAnalysisService
    {
        Task<Response<bool>> AnalyseDatabase(Guid DatabaseConnectionId);
    }
}
