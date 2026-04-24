using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    public partial class SustentoSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoSustento",
                table: "Herois",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoSustentoEm",
                table: "Cidades",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoSustento",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "UltimoSustentoEm",
                table: "Cidades");
        }
    }
}
