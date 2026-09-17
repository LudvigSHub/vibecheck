using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCheck.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRankedQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankedAttempts",
                columns: table => new
                {
                    RankedAttemptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: true),
                    WrongCount = table.Column<int>(type: "int", nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedAttempts", x => x.RankedAttemptID);
                    table.ForeignKey(
                        name: "FK_RankedAttempts_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    LeaderboardEntryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RankedAttemptID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.LeaderboardEntryID);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_RankedAttempts_RankedAttemptID",
                        column: x => x.RankedAttemptID,
                        principalTable: "RankedAttempts",
                        principalColumn: "RankedAttemptID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankedAttemptAnswers",
                columns: table => new
                {
                    RankedAttemptAnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankedAttemptID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    SelectedAlternativeID = table.Column<int>(type: "int", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankedAttemptAnswers", x => x.RankedAttemptAnswerID);
                    table.ForeignKey(
                        name: "FK_RankedAttemptAnswers_QuestionAlternatives_QuestionID_SelectedAlternativeID",
                        columns: x => new { x.QuestionID, x.SelectedAlternativeID },
                        principalTable: "QuestionAlternatives",
                        principalColumns: new[] { "QuestionID", "AlternativeID" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RankedAttemptAnswers_Questions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RankedAttemptAnswers_RankedAttempts_RankedAttemptID",
                        column: x => x.RankedAttemptID,
                        principalTable: "RankedAttempts",
                        principalColumn: "RankedAttemptID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_RankedAttemptID",
                table: "LeaderboardEntries",
                column: "RankedAttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_UserID_WeekStart",
                table: "LeaderboardEntries",
                columns: new[] { "UserID", "WeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_WeekStart_Score_AchievedAt",
                table: "LeaderboardEntries",
                columns: new[] { "WeekStart", "Score", "AchievedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RankedAttemptAnswers_QuestionID_SelectedAlternativeID",
                table: "RankedAttemptAnswers",
                columns: new[] { "QuestionID", "SelectedAlternativeID" });

            migrationBuilder.CreateIndex(
                name: "IX_RankedAttemptAnswers_RankedAttemptID_QuestionID",
                table: "RankedAttemptAnswers",
                columns: new[] { "RankedAttemptID", "QuestionID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankedAttempts_UserID",
                table: "RankedAttempts",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "RankedAttemptAnswers");

            migrationBuilder.DropTable(
                name: "RankedAttempts");
        }
    }
}
