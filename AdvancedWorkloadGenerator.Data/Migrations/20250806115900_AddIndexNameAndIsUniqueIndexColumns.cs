using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvancedWorkloadGenerator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexNameAndIsUniqueIndexColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndexName",
                table: "Columns",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUniqueIndex",
                table: "Columns",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndexName",
                table: "Columns");

            migrationBuilder.DropColumn(
                name: "IsUniqueIndex",
                table: "Columns");
        }
    }
}
