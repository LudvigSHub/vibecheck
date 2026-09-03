using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCheck.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionWord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WordID",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_WordID",
                table: "Questions",
                column: "WordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Words_WordID",
                table: "Questions",
                column: "WordID",
                principalTable: "Words",
                principalColumn: "WordID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Words_WordID",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_WordID",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "WordID",
                table: "Questions");
        }
    }
}
