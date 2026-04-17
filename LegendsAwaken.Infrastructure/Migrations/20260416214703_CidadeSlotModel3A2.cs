using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CidadeSlotModel3A2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResourceNode",
                table: "PersonagemTrabalhador",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Confianca",
                table: "Herois",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Humor",
                table: "Herois",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Lore",
                table: "Herois",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Treinamento_UltimoTreino",
                table: "Herois",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPredio",
                table: "Construcao",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SlotOcupacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConstrucaoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SlotTipo = table.Column<int>(type: "INTEGER", nullable: false),
                    PosicaoSlot = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotOcupacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlotOcupacoes_Construcao_ConstrucaoId",
                        column: x => x.ConstrucaoId,
                        principalTable: "Construcao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SlotOcupacoes_ConstrucaoId",
                table: "SlotOcupacoes",
                column: "ConstrucaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SlotOcupacoes");

            migrationBuilder.DropColumn(
                name: "ResourceNode",
                table: "PersonagemTrabalhador");

            migrationBuilder.DropColumn(
                name: "Confianca",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "Humor",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "Lore",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "Treinamento_UltimoTreino",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "TipoPredio",
                table: "Construcao");
        }
    }
}
