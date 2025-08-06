using System;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AdvancedWorkloadGenerator.Data.Extensions;
using Serilog;
using Serilog.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AdvancedWorkloadGenerator
{
    internal class Program
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureServices();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static void ConfigureServices()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: "Logs/AdvancedWorkloadGenerator-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger);
            });
            
            // Add data services with SQLite connection string
            string connectionString = "Data Source=AdvancedWorkloadGenerator.db";
            services.AddDataServices(connectionString);
            
            ServiceProvider = services.BuildServiceProvider();
            
            // Ensure database is created
            EnsureDatabaseCreated();
        }
        
        private static void EnsureDatabaseCreated()
        {
            try
            {
                using var scope = ServiceProvider?.CreateScope();
                var dbContext = scope?.ServiceProvider.GetService<AdvancedWorkloadGenerator.Data.Context.GeneratorDbContext>();
                var logger = scope?.ServiceProvider.GetService<ILogger<Program>>();
                
                if (dbContext != null)
                {
                    // Check for pending migrations
                    var pendingMigrations = dbContext.Database.GetPendingMigrations();
                    
                    if (pendingMigrations.Any())
                    {
                        logger?.LogInformation("Found pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
                        logger?.LogInformation("Applying pending migrations...");
                        
                        // Apply pending migrations
                        dbContext.Database.Migrate();
                        
                        logger?.LogInformation("All migrations applied successfully");
                    }
                    else
                    {
                        // Ensure database is created if it doesn't exist
                        dbContext.Database.EnsureCreated();
                        logger?.LogInformation("Database is up to date");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't crash the application
                Log.Logger.Error(ex, "Database initialization failed");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
