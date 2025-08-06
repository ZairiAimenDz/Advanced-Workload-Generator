using AdvancedWorkloadGenerator.Core.Interfaces;
using AdvancedWorkloadGenerator.Data.Context;
using AdvancedWorkloadGenerator.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdvancedWorkloadGenerator.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
        {
            // Register DbContext
            services.AddDbContext<GeneratorDbContext>(options =>
                options.UseSqlite(connectionString));

            // Register services
            services.AddScoped<IConnectionStringService, ConnectionStringService>();
            services.AddScoped<IDatabaseAnalysisService, DatabaseAnalysisService>();
            services.AddScoped<IQueryGenerator, QueryGenerator>();

            return services;
        }
    }
}