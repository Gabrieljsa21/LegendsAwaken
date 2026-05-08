using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtributosDnD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename old attribute columns to D&D names (data-preserving)
            migrationBuilder.RenameColumn(
                name: "AtributosBase_Agilidade",
                table: "Herois",
                newName: "AtributosBase_Destreza");

            migrationBuilder.RenameColumn(
                name: "AtributosBase_Vitalidade",
                table: "Herois",
                newName: "AtributosBase_Constituicao");

            migrationBuilder.RenameColumn(
                name: "AtributosBase_Percepcao",
                table: "Herois",
                newName: "AtributosBase_Sabedoria");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Agilidade",
                table: "Herois",
                newName: "AtributosDistribuidos_Destreza");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Vitalidade",
                table: "Herois",
                newName: "AtributosDistribuidos_Constituicao");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Percepcao",
                table: "Herois",
                newName: "AtributosDistribuidos_Sabedoria");

            // Add new Carisma columns (no prior equivalent)
            migrationBuilder.AddColumn<int>(
                name: "AtributosBase_Carisma",
                table: "Herois",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AtributosDistribuidos_Carisma",
                table: "Herois",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Create HeroisPericias table
            migrationBuilder.CreateTable(
                name: "HeroisPericias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pericia = table.Column<int>(type: "INTEGER", nullable: false),
                    TemProficiencia = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroisPericias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeroisPericias_Herois_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "Herois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeroisPericias_HeroiId_Pericia",
                table: "HeroisPericias",
                columns: new[] { "HeroiId", "Pericia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeroisPericias");

            migrationBuilder.DropColumn(
                name: "AtributosBase_Carisma",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "AtributosDistribuidos_Carisma",
                table: "Herois");

            migrationBuilder.RenameColumn(
                name: "AtributosBase_Destreza",
                table: "Herois",
                newName: "AtributosBase_Agilidade");

            migrationBuilder.RenameColumn(
                name: "AtributosBase_Constituicao",
                table: "Herois",
                newName: "AtributosBase_Vitalidade");

            migrationBuilder.RenameColumn(
                name: "AtributosBase_Sabedoria",
                table: "Herois",
                newName: "AtributosBase_Percepcao");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Destreza",
                table: "Herois",
                newName: "AtributosDistribuidos_Agilidade");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Constituicao",
                table: "Herois",
                newName: "AtributosDistribuidos_Vitalidade");

            migrationBuilder.RenameColumn(
                name: "AtributosDistribuidos_Sabedoria",
                table: "Herois",
                newName: "AtributosDistribuidos_Percepcao");
        }
    }
}
