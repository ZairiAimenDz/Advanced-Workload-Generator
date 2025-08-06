using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Models.Wrappers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Interfaces
{
    public interface IConnectionStringService
    {
        Task<Response<List<DatabaseConnectionDTO>>> GetAllConnectionStrings();
        Task<Response<DatabaseConnectionDetailedDTO>> GetConnectionStringById(Guid id);
        Task<Response<DatabaseConnectionDetailedDTO>> CreateConnectionString(DatabaseConnectionCreateDTO connectionString);
        Task<Response<DatabaseConnectionDTO>> UpdateConnectionString(Guid id, DatabaseConnectionCreateDTO connectionString);
        Task<Response<bool>> DeleteConnectionString(Guid id);
    }
}
