using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaTagastus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaadudBoonus",
                table: "Muugid",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Tagastatud",
                table: "Muugid",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TagastatudAeg",
                table: "Muugid",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KlientId",
                table: "Kasutajad",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MuugiRead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuukId = table.Column<int>(type: "int", nullable: false),
                    ToodeId = table.Column<int>(type: "int", nullable: false),
                    Toode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Kogus = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Summa = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuugiRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuugiRead_Muugid_MuukId",
                        column: x => x.MuukId,
                        principalTable: "Muugid",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuugiRead_MuukId",
                table: "MuugiRead",
                column: "MuukId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuugiRead");

            migrationBuilder.DropColumn(
                name: "SaadudBoonus",
                table: "Muugid");

            migrationBuilder.DropColumn(
                name: "Tagastatud",
                table: "Muugid");

            migrationBuilder.DropColumn(
                name: "TagastatudAeg",
                table: "Muugid");

            migrationBuilder.DropColumn(
                name: "KlientId",
                table: "Kasutajad");
        }
    }
}
