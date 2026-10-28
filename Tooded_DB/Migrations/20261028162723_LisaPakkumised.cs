using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaPakkumised : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SoodusAlates",
                table: "Toodetabel",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Maksetuup",
                table: "Muugid",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Pakkumised",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tuup = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ToodeId = table.Column<int>(type: "int", nullable: false),
                    Kogus = table.Column<int>(type: "int", nullable: false),
                    Tasuta = table.Column<int>(type: "int", nullable: false),
                    KomplektHind = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Toode2Id = table.Column<int>(type: "int", nullable: true),
                    Protsent = table.Column<int>(type: "int", nullable: false),
                    Alates = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kuni = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pakkumised", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pakkumised_Toodetabel_Toode2Id",
                        column: x => x.Toode2Id,
                        principalTable: "Toodetabel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pakkumised_Toodetabel_ToodeId",
                        column: x => x.ToodeId,
                        principalTable: "Toodetabel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pakkumised_Toode2Id",
                table: "Pakkumised",
                column: "Toode2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pakkumised_ToodeId",
                table: "Pakkumised",
                column: "ToodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pakkumised");

            migrationBuilder.DropColumn(
                name: "SoodusAlates",
                table: "Toodetabel");

            migrationBuilder.AlterColumn<string>(
                name: "Maksetuup",
                table: "Muugid",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
