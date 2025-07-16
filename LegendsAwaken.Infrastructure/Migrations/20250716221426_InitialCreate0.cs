using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Andares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    TemBoss = table.Column<bool>(type: "INTEGER", nullable: false),
                    DificuldadeBoss = table.Column<int>(type: "INTEGER", nullable: true),
                    RecompensaTipo = table.Column<string>(type: "TEXT", nullable: false),
                    RecompensaQuantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ObjetivoCumprido = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Andares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cidades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    Populacao = table.Column<int>(type: "INTEGER", nullable: false),
                    CapacidadeMaxima = table.Column<int>(type: "INTEGER", nullable: false),
                    Recursos_Comida = table.Column<int>(type: "INTEGER", nullable: false),
                    Recursos_Madeira = table.Column<int>(type: "INTEGER", nullable: false),
                    Recursos_Pedra = table.Column<int>(type: "INTEGER", nullable: false),
                    Recursos_Ouro = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Herois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Raridade = table.Column<int>(type: "INTEGER", nullable: false),
                    Raca = table.Column<string>(type: "TEXT", nullable: false),
                    Classe = table.Column<string>(type: "TEXT", nullable: false),
                    Antecedente = table.Column<string>(type: "TEXT", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    XP = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Forca = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Destreza = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Constituicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Inteligencia = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Sabedoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Carisma = table.Column<int>(type: "INTEGER", nullable: false),
                    Status_VidaAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    Status_VidaMaxima = table.Column<int>(type: "INTEGER", nullable: false),
                    Status_ManaAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    Status_ManaMaxima = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipamentos_Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Equipamentos_Arma = table.Column<string>(type: "TEXT", nullable: true),
                    Equipamentos_Armadura = table.Column<string>(type: "TEXT", nullable: true),
                    Equipamentos_Acessorios = table.Column<string>(type: "TEXT", nullable: false),
                    Treinamento_Tipo = table.Column<string>(type: "TEXT", nullable: true),
                    Treinamento_Inicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Treinamento_Fim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Treinamento_ResultadoEsperado = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    DataInvocacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herois", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    NivelConta = table.Column<int>(type: "INTEGER", nullable: false),
                    XP = table.Column<int>(type: "INTEGER", nullable: false),
                    Moedas = table.Column<int>(type: "INTEGER", nullable: false),
                    Cristais = table.Column<int>(type: "INTEGER", nullable: false),
                    Fragmentos = table.Column<int>(type: "INTEGER", nullable: false),
                    PergaminhosSecretos = table.Column<int>(type: "INTEGER", nullable: false),
                    HeroisIds = table.Column<string>(type: "TEXT", nullable: false),
                    CidadeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AndarMaisAlto = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.DiscordId);
                });

            migrationBuilder.CreateTable(
                name: "Inimigo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Atributos_Forca = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Destreza = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Constituicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Inteligencia = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Sabedoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Atributos_Carisma = table.Column<int>(type: "INTEGER", nullable: false),
                    Habilidades = table.Column<string>(type: "TEXT", nullable: false),
                    TorreAndarId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inimigo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inimigo_Andares_TorreAndarId",
                        column: x => x.TorreAndarId,
                        principalTable: "Andares",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Construcao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    EstaAtiva = table.Column<bool>(type: "INTEGER", nullable: false),
                    CidadeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Construcao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Construcao_Cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "Cidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonagemTrabalhador",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Profissao = table.Column<string>(type: "TEXT", nullable: false),
                    InicioTrabalho = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TerminoTrabalho = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CidadeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonagemTrabalhador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonagemTrabalhador_Cidades_CidadeId",
                        column: x => x.CidadeId,
                        principalTable: "Cidades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Herois_Ativas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HabilidadeContainerHeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    XPAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    XPMaximo = table.Column<int>(type: "INTEGER", nullable: false),
                    EstaEmTreinamento = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herois_Ativas", x => new { x.HabilidadeContainerHeroiId, x.Id });
                    table.ForeignKey(
                        name: "FK_Herois_Ativas_Herois_HabilidadeContainerHeroiId",
                        column: x => x.HabilidadeContainerHeroiId,
                        principalTable: "Herois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Herois_Passivas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    HabilidadeContainerHeroiId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    XPAtual = table.Column<int>(type: "INTEGER", nullable: false),
                    XPMaximo = table.Column<int>(type: "INTEGER", nullable: false),
                    EstaEmTreinamento = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Herois_Passivas", x => new { x.HabilidadeContainerHeroiId, x.Id });
                    table.ForeignKey(
                        name: "FK_Herois_Passivas_Herois_HabilidadeContainerHeroiId",
                        column: x => x.HabilidadeContainerHeroiId,
                        principalTable: "Herois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Construcao_CidadeId",
                table: "Construcao",
                column: "CidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Inimigo_TorreAndarId",
                table: "Inimigo",
                column: "TorreAndarId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonagemTrabalhador_CidadeId",
                table: "PersonagemTrabalhador",
                column: "CidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Construcao");

            migrationBuilder.DropTable(
                name: "Herois_Ativas");

            migrationBuilder.DropTable(
                name: "Herois_Passivas");

            migrationBuilder.DropTable(
                name: "Inimigo");

            migrationBuilder.DropTable(
                name: "PersonagemTrabalhador");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Herois");

            migrationBuilder.DropTable(
                name: "Andares");

            migrationBuilder.DropTable(
                name: "Cidades");
        }
    }
}
