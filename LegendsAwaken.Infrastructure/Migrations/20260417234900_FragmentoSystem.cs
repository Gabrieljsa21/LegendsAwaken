using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FragmentoSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Biomas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    AndarInicio = table.Column<int>(type: "INTEGER", nullable: false),
                    AndarFim = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Biomas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeroiConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    RaridadeBase = table.Column<int>(type: "INTEGER", nullable: false),
                    Arquetipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroiConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BiomHeroPools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BiomeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Raridade = table.Column<int>(type: "INTEGER", nullable: false),
                    DropWeight = table.Column<int>(type: "INTEGER", nullable: false),
                    EHeroPrincipal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiomHeroPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BiomHeroPools_Biomas_BiomeId",
                        column: x => x.BiomeId,
                        principalTable: "Biomas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BiomHeroPools_HeroiConfigs_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "HeroiConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Arquetipo = table.Column<int>(type: "INTEGER", nullable: true),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_HeroiConfigs_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "HeroiConfigs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FragmentosProgresso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoFragmento = table.Column<int>(type: "INTEGER", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Arquetipo = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FragmentosProgresso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FragmentosProgresso_HeroiConfigs_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "HeroiConfigs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HeroisDesbloqueados",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DesbloqueadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroisDesbloqueados", x => new { x.UsuarioId, x.HeroiId });
                    table.ForeignKey(
                        name: "FK_HeroisDesbloqueados_HeroiConfigs_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "HeroiConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeroiUnlockConfigs",
                columns: table => new
                {
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoUnlock = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeFragmentos = table.Column<int>(type: "INTEGER", nullable: true),
                    AndarMarco = table.Column<int>(type: "INTEGER", nullable: true),
                    CondicaoDescricao = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroiUnlockConfigs", x => x.HeroiId);
                    table.ForeignKey(
                        name: "FK_HeroiUnlockConfigs_HeroiConfigs_HeroiId",
                        column: x => x.HeroiId,
                        principalTable: "HeroiConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Biomas",
                columns: new[] { "Id", "AndarFim", "AndarInicio", "Descricao", "Nome", "Tag" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000001"), 10, 1, "Uma floresta antiga onde aventureiros escrevem suas primeiras histórias.", "Floresta de Aelindra", "Floresta" },
                    { new Guid("b1000000-0000-0000-0000-000000000002"), 25, 11, "Ruínas de uma civilização esquecida, repletas de armadilhas e segredos.", "Ruínas de Valdrek", "Ruinas" },
                    { new Guid("b1000000-0000-0000-0000-000000000003"), 50, 26, "O cume incandescente onde os guerreiros mais duros são forjados.", "Pico Vulcânico", "Vulcanico" }
                });

            migrationBuilder.InsertData(
                table: "HeroiConfigs",
                columns: new[] { "Id", "Arquetipo", "Nome", "RaridadeBase", "Tag" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 0, "Aldric, o Sem-Corrente", 5, null },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 2, "Yuzara, a Tecelã do Destino", 5, null },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), 12, "Thorvald, o Arquiteto das Eras", 5, null },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 1, "Kaen", 4, null },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), 3, "Nyra", 4, null },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), 4, "Seraph", 4, null },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), 15, "Mira", 4, null },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), 10, "Grom", 4, null },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), 11, "Hana", 4, null }
                });

            migrationBuilder.InsertData(
                table: "BiomHeroPools",
                columns: new[] { "Id", "BiomeId", "DropWeight", "EHeroPrincipal", "HeroiId", "Raridade" },
                values: new object[,]
                {
                    { new Guid("c1000000-0000-0000-0000-000000000001"), new Guid("b1000000-0000-0000-0000-000000000001"), 30, true, new Guid("a1000000-0000-0000-0000-000000000004"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000002"), new Guid("b1000000-0000-0000-0000-000000000001"), 70, false, new Guid("a1000000-0000-0000-0000-000000000009"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000003"), new Guid("b1000000-0000-0000-0000-000000000002"), 30, true, new Guid("a1000000-0000-0000-0000-000000000006"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000004"), new Guid("b1000000-0000-0000-0000-000000000002"), 70, false, new Guid("a1000000-0000-0000-0000-000000000005"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000005"), new Guid("b1000000-0000-0000-0000-000000000003"), 20, true, new Guid("a1000000-0000-0000-0000-000000000001"), 5 },
                    { new Guid("c1000000-0000-0000-0000-000000000006"), new Guid("b1000000-0000-0000-0000-000000000003"), 45, false, new Guid("a1000000-0000-0000-0000-000000000007"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000007"), new Guid("b1000000-0000-0000-0000-000000000003"), 35, false, new Guid("a1000000-0000-0000-0000-000000000008"), 4 }
                });

            migrationBuilder.InsertData(
                table: "HeroiUnlockConfigs",
                columns: new[] { "HeroiId", "AndarMarco", "CondicaoDescricao", "QuantidadeFragmentos", "TipoUnlock" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 30, null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 60, null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), null, null, 60, 1 },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 10, null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), null, "Completar o andar 15 com a party completa sem nenhum herói ser derrotado", null, 3 },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), null, null, 40, 1 },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), null, null, 35, 1 },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), null, null, 30, 1 },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), null, "Ter pelo menos 3 heróis com Humor >= 80 na cidade ao mesmo tempo", null, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BiomHeroPools_BiomeId",
                table: "BiomHeroPools",
                column: "BiomeId");

            migrationBuilder.CreateIndex(
                name: "IX_BiomHeroPools_HeroiId",
                table: "BiomHeroPools",
                column: "HeroiId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_HeroiId",
                table: "Contratos",
                column: "HeroiId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_UsuarioId_Tipo_Ativo",
                table: "Contratos",
                columns: new[] { "UsuarioId", "Tipo", "Ativo" },
                unique: true,
                filter: "\"Ativo\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_HeroiId",
                table: "FragmentosProgresso",
                column: "HeroiId");

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_Arquetipo",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "Arquetipo" });

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_HeroiId",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "HeroiId" });

            migrationBuilder.CreateIndex(
                name: "IX_HeroisDesbloqueados_HeroiId",
                table: "HeroisDesbloqueados",
                column: "HeroiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BiomHeroPools");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "FragmentosProgresso");

            migrationBuilder.DropTable(
                name: "HeroisDesbloqueados");

            migrationBuilder.DropTable(
                name: "HeroiUnlockConfigs");

            migrationBuilder.DropTable(
                name: "Biomas");

            migrationBuilder.DropTable(
                name: "HeroiConfigs");
        }
    }
}
