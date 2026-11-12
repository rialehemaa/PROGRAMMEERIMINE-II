using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaKasutajad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kasutajad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kasutajanimi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParoolHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Roll = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kasutajad", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kasutajad_Kasutajanimi",
                table: "Kasutajad",
                column: "Kasutajanimi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kasutajad");
        }
    }
}
