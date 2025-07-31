using AdvancedWorkloadGenerator.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AdvancedWorkloadGenerator.Core.Entities
{
    // Query Generation Request Entity
    public class QueryGenerationRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string RequestName { get; set; } = string.Empty;
        public QueryGenerationParameters Parameters { get; set; }

        public int GeneratedQueries { get; set; } = 0;
        public int ExecutedQueries { get; set; } = 0;

        public RunningStatus GenerationStatus { get; set; } = RunningStatus.Pending;
        public RunningStatus ExecutionStatus { get; set; } = RunningStatus.NotInitiated;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedExecutionAt { get; set; }
        public DateTime? FinishedExecutionAt { get; set; }

        public DateTime? StartedGenerationAt { get; set; }
        public DateTime? FinishedGenerationAt { get; set; }

        public string? ErrorMessage { get; set; }
        public string? ResultFilePath { get; set; }
        public string? ResultCSVPath { get; set; }

        // Navigation properties
        public Guid DatabaseConnectionId { get; set; }
        public virtual DBConnection DatabaseConnection { get; set; }
    }
}
