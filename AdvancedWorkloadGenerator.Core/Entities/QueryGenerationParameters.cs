using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWorkloadGenerator.Core.Entities
{
    public struct QueryGenerationParameters
    {
        public QueryGenerationParameters()
        {}

        /// <summary>
        /// How Many Queries To Generate
        /// </summary>
        public int NumQueries { get; set; } = 10000;
        /// <summary>
        /// Maximum Number of Predicates in a Query (ex. Operations In The Where Clause)
        /// </summary>
        public int MaxNoPredicates { get; set; } = 5;
        /// <summary>
        /// Maximum Number of Aggregates in a Query (ex. Count, Sum, Avg, etc.)
        /// </summary>
        public int MaxNoAggregates { get; set; } = 3;
        /// <summary>
        /// Seed for Random Number Generation
        /// </summary>
        public int Seed { get; set; } = 1;
        /// <summary>
        /// Maximum Number of Joins in a Query
        /// </summary>
        public int MaxJoins { get; set; } = 10;


        /// <summary>
        /// Use Advanced Queries (ex. Index, Physical Operators)
        /// </summary>
        public bool AdvancedQueries { get; set; } = true;
        /// <summary>
        /// Use Index Hints (ex. Force Index, Use Index, etc.)
        /// </summary>
        public bool UseIndexHints { get; set; } = true;
        /// <summary>
        /// Probability of Using Index Hints
        /// </summary>
        public double IndexHintProbability { get; set; } = 0.3;
        /// <summary>
        /// Probability of Forcing No Index Hint
        /// </summary>
        public double NoIndexHintProbability { get; set; } = 0.2;
        /// <summary>
        /// Use Physical Operator Hints (ex. Loop Join, Hash Join, etc.)
        /// </summary>
        public bool UsePhysicalOperatorHints { get; set; } = true;
        /// <summary>
        /// Probability of Using Physical Operator Hints
        /// </summary>
        public double PhysicalOperatorHintProbability { get; set; } = 0.25;
    }
}
