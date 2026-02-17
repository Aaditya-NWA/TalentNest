using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequirementService.Migrations
{
    /// <inheritdoc />
    public partial class RequirementInterview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Project = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SkillsNeeded = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    AvailabilityWindow = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientInterviewRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_Availability",
                table: "Requirements",
                column: "AvailabilityWindow");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_Project",
                table: "Requirements",
                column: "Project");

            migrationBuilder.CreateIndex(
                name: "IX_Requirements_Skills",
                table: "Requirements",
                column: "SkillsNeeded");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requirements");
        }
    }
}
