using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaKliendid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kliendid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nimi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Kliendikaart = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Boonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kliendid", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kliendid_Kliendikaart",
                table: "Kliendid",
                column: "Kliendikaart",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kliendid");
        }
    }
}
