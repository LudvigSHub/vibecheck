using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCheck.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizAttemptQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizAttemptQuestions",
                columns: table => new
                {
                    QuizAttemptID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptQuestions", x => new { x.QuizAttemptID, x.QuestionID });
                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_Questions_QuestionID",
                        column: x => x.QuestionID,
                        principalTable: "Questions",
                        principalColumn: "QuestionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_QuizAttempts_QuizAttemptID",
                        column: x => x.QuizAttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptQuestions_QuestionID",
                table: "QuizAttemptQuestions",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptQuestions_QuizAttemptID_Order",
                table: "QuizAttemptQuestions",
                columns: new[] { "QuizAttemptID", "Order" },
                unique: true);

            // Preserve the fixed question list for attempts already in progress.
            // Completed scores are left untouched. Apply before changing old quiz links.
            migrationBuilder.Sql("""
                INSERT INTO [QuizAttemptQuestions] ([QuizAttemptID], [QuestionID], [Order])
                SELECT a.[QuizAttemptID], q.[QuestionID],
                       ROW_NUMBER() OVER (PARTITION BY a.[QuizAttemptID] ORDER BY q.[QuizQuestionID])
                FROM [QuizAttempts] a
                INNER JOIN [QuizQuestions] q ON q.[QuizID] = a.[QuizID]
                WHERE a.[CompletedAt] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAttemptQuestions");
        }
    }
}
