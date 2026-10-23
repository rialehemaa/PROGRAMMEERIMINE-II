using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tooded_DB.Migrations
{
    /// <inheritdoc />
    public partial class LisaSoodused : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Soodus",
                table: "Toodetabel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SoodusKuni",
                table: "Toodetabel",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Maksetuup",
                table: "Muugid",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SynnipaevaSoodus",
                table: "Muugid",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "Synnipaev",
                table: "Kliendid",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Soodus",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "SoodusKuni",
                table: "Toodetabel");

            migrationBuilder.DropColumn(
                name: "Maksetuup",
                table: "Muugid");

            migrationBuilder.DropColumn(
                name: "SynnipaevaSoodus",
                table: "Muugid");

            migrationBuilder.DropColumn(
                name: "Synnipaev",
                table: "Kliendid");
        }
    }
}
