using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Core.Models.DatabaseConnections;
using AdvancedWorkloadGenerator.Core.Models.Wrappers;
using AdvancedWorkloadGenerator.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Data.Services
{
    public class ConnectionStringService(GeneratorDbContext context) : IConnectionStringService
    {
        public Task<Response<DatabaseConnectionDetailedDTO>> CreateConnectionString(DatabaseConnectionCreateDTO connectionString)
        {
            throw new NotImplementedException();
        }

        public Task<Response<bool>> DeleteConnectionString(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<List<DatabaseConnectionDTO>>> GetAllConnectionStrings()
        {
            throw new NotImplementedException();
        }

        public Task<Response<DatabaseConnectionDetailedDTO>> GetConnectionStringById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Response<DatabaseConnectionDTO>> UpdateConnectionString(Guid id, DatabaseConnectionCreateDTO connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
