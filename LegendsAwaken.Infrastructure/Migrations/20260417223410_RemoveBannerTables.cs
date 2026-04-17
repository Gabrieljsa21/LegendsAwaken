using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBannerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannerHistorico");

            migrationBuilder.DropTable(
                name: "BannerProgressos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BannerHistorico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BannerId = table.Column<string>(type: "TEXT", nullable: false),
                    DataUltimoReset = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuantidadeInvocacoes = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannerHistorico_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BannerProgressos",
                columns: table => new
                {
                    UsuarioId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BannerId = table.Column<string>(type: "TEXT", nullable: false),
                    ChanceHumano = table.Column<int>(type: "INTEGER", nullable: false),
                    OutrasChances = table.Column<string>(type: "TEXT", nullable: false),
                    ProximoIndexCrescente = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeRolls = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannerProgressos", x => new { x.UsuarioId, x.BannerId });
                    table.ForeignKey(
                        name: "FK_BannerProgressos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannerHistorico_UsuarioId",
                table: "BannerHistorico",
                column: "UsuarioId");
        }
    }
}
