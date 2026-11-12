using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaMuugid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Muugid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kuupaev = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KasutajaId = table.Column<int>(type: "int", nullable: true),
                    KlientId = table.Column<int>(type: "int", nullable: true),
                    Kokku = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    KasutatudBoonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Makstud = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Arve = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muugid", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muugid_Kasutajad_KasutajaId",
                        column: x => x.KasutajaId,
                        principalTable: "Kasutajad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Muugid_Kliendid_KlientId",
                        column: x => x.KlientId,
                        principalTable: "Kliendid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Muugid_KasutajaId",
                table: "Muugid",
                column: "KasutajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Muugid_KlientId",
                table: "Muugid",
                column: "KlientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Muugid");
        }
    }
}
