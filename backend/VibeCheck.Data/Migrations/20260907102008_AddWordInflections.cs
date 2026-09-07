using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCheck.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWordInflections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InflectionTypes",
                columns: table => new
                {
                    InflectionTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InflectionTypes", x => x.InflectionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "WordInflections",
                columns: table => new
                {
                    WordInflectionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WordID = table.Column<int>(type: "int", nullable: false),
                    InflectionTypeID = table.Column<int>(type: "int", nullable: false),
                    InflectedText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WordInflections", x => x.WordInflectionID);
                    table.ForeignKey(
                        name: "FK_WordInflections_InflectionTypes_InflectionTypeID",
                        column: x => x.InflectionTypeID,
                        principalTable: "InflectionTypes",
                        principalColumn: "InflectionTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WordInflections_Words_WordID",
                        column: x => x.WordID,
                        principalTable: "Words",
                        principalColumn: "WordID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InflectionTypes_Code",
                table: "InflectionTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WordInflections_InflectionTypeID",
                table: "WordInflections",
                column: "InflectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_WordInflections_WordID",
                table: "WordInflections",
                column: "WordID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WordInflections");

            migrationBuilder.DropTable(
                name: "InflectionTypes");
        }
    }
}
