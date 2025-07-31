using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Interfaces
{
    public interface IQueryGenerator
    {
        Task GenerateQueriesAsync(Guid QueryGenRequest);
        Task<string> GenerateSingleQueryAsync(Guid DatabaseConnectionID);
    }
}
