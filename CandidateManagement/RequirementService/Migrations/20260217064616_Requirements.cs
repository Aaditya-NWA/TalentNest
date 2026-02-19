using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace RequirementService.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class Requirements : Migration
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
                    Project = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SkillsNeeded = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    MaxExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    AvailabilityStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailabilityEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientInterviewRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requirements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requirements");
        }
    }
}
