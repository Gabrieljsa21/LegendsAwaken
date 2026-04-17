using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CraftingV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equipamentos_Acessorios",
                table: "Herois");

            migrationBuilder.RenameColumn(
                name: "Equipamentos_Armadura",
                table: "Herois",
                newName: "Equipamentos_ArmaduraId");

            migrationBuilder.RenameColumn(
                name: "Equipamentos_Arma",
                table: "Herois",
                newName: "Equipamentos_ArmaId");

            migrationBuilder.AddColumn<Guid>(
                name: "Equipamentos_AcessorioId",
                table: "Herois",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "HeroiBonusAtributo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Itens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Slot = table.Column<int>(type: "INTEGER", nullable: false),
                    Qualidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ProprietarioId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EstaEquipado = table.Column<bool>(type: "INTEGER", nullable: false),
                    HeroiEquipadoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemBonus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ItemId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Atributo = table.Column<int>(type: "INTEGER", nullable: false),
                    Valor = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBonus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemBonus_Itens_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Itens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemBonus_ItemId",
                table: "ItemBonus",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemBonus");

            migrationBuilder.DropTable(
                name: "Itens");

            migrationBuilder.DropColumn(
                name: "Equipamentos_AcessorioId",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "HeroiBonusAtributo");

            migrationBuilder.RenameColumn(
                name: "Equipamentos_ArmaduraId",
                table: "Herois",
                newName: "Equipamentos_Armadura");

            migrationBuilder.RenameColumn(
                name: "Equipamentos_ArmaId",
                table: "Herois",
                newName: "Equipamentos_Arma");

            migrationBuilder.AddColumn<string>(
                name: "Equipamentos_Acessorios",
                table: "Herois",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
