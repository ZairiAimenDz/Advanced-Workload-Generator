using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvancedWorkloadGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DatabaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ConnectionString = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Host = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    DatabaseName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    LastAnalyzedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsAnalyzed = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenerationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    GeneratedQueries = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ExecutedQueries = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    GenerationStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ExecutionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartedExecutionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedExecutionAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    StartedGenerationAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FinishedGenerationAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ResultFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ResultCSVPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DatabaseConnectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parameters_AdvancedQueries = table.Column<bool>(type: "INTEGER", nullable: false),
                    Parameters_IndexHintProbability = table.Column<double>(type: "REAL", nullable: false),
                    Parameters_MaxJoins = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters_MaxNoAggregates = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters_MaxNoPredicates = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters_NoIndexHintProbability = table.Column<double>(type: "REAL", nullable: false),
                    Parameters_NumQueries = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters_PhysicalOperatorHintProbability = table.Column<double>(type: "REAL", nullable: false),
                    Parameters_Seed = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters_UseIndexHints = table.Column<bool>(type: "INTEGER", nullable: false),
                    Parameters_UsePhysicalOperatorHints = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenerationRequests_Connections_DatabaseConnectionId",
                        column: x => x.DatabaseConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Schema = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    RecordCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DatabaseConnectionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tables_Connections_DatabaseConnectionId",
                        column: x => x.DatabaseConnectionId,
                        principalTable: "Connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Columns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AttributeName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MaxLength = table.Column<int>(type: "INTEGER", nullable: true),
                    IsNullable = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    IsPrimaryKey = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsForeignKey = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsUnique = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    HasIndex = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OrdinalPosition = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferencedTableName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ReferencedAttributeName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ReferencedTableId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TableId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Columns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Columns_Tables_ReferencedTableId",
                        column: x => x.ReferencedTableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Columns_Tables_TableId",
                        column: x => x.TableId,
                        principalTable: "Tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Columns_ReferencedTableId",
                table: "Columns",
                column: "ReferencedTableId");

            migrationBuilder.CreateIndex(
                name: "IX_Columns_TableId",
                table: "Columns",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerationRequests_DatabaseConnectionId",
                table: "GenerationRequests",
                column: "DatabaseConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_DatabaseConnectionId",
                table: "Tables",
                column: "DatabaseConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Columns");

            migrationBuilder.DropTable(
                name: "GenerationRequests");

            migrationBuilder.DropTable(
                name: "Tables");

            migrationBuilder.DropTable(
                name: "Connections");
        }
    }
}
