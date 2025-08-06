using AdvancedWorkloadGenerator.Core.Models.Wrappers;
using Microsoft.Extensions.Logging;

namespace AdvancedWorkloadGenerator.Core.Interfaces
{
    public interface IDatabaseAnalysisService
    {
        Task<Response<bool>> AnalyseDatabase(Guid DatabaseConnectionId);
    }
}
