using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable disable


namespace InterviewService.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    RequirementId = table.Column<int>(type: "int", nullable: false),
                    Project = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Account = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Interviewer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    FinalOutcome = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DecisionMaker = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OutcomeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.CheckConstraint("CK_Interview_DecisionMaker", "[DecisionMaker] IN (0, 1, 2, 3)");
                    table.CheckConstraint("CK_Interview_Level", "[Level] IN (1, 2)");
                    table.CheckConstraint("CK_Interview_Outcome", "[FinalOutcome] IN (0, 1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InterviewId = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TechnicalScore = table.Column<int>(type: "int", nullable: false),
                    CommunicationScore = table.Column<int>(type: "int", nullable: false),
                    RecommendedOutcome = table.Column<int>(type: "int", nullable: false),
                    Strengths = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Weaknesses = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Interviews_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "Interviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_InterviewId",
                table: "Feedbacks",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_Candidate_Project_Date",
                table: "Interviews",
                columns: new[] { "CandidateId", "Project", "InterviewDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_CandidateId",
                table: "Interviews",
                column: "CandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_InterviewDate",
                table: "Interviews",
                column: "InterviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_RequirementId",
                table: "Interviews",
                column: "RequirementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Interviews");
        }
    }
}
