using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroTitulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "Herois",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "HeroiConfigs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000001"),
                column: "AndarFim",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000002"),
                columns: new[] { "AndarFim", "AndarInicio" },
                values: new object[] { 50, 26 });

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000003"),
                columns: new[] { "AndarFim", "AndarInicio" },
                values: new object[] { 75, 51 });

            migrationBuilder.InsertData(
                table: "Biomas",
                columns: new[] { "Id", "AndarFim", "AndarInicio", "Descricao", "Nome", "Tag" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000004"), 100, 76, "Um abismo de trevas onde anjos caídos e magos do vazio travam suas guerras eternas.", "Abismo Sombrio", "Abismo" },
                    { new Guid("b1000000-0000-0000-0000-000000000005"), 125, 101, "O palco final do conflito entre Serafins e Anjos Caídos pelo destino do mundo mortal.", "Domínio Celestial", "Celestial" }
                });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                columns: new[] { "Nome", "Titulo" },
                values: new object[] { "Aldric", "o Sem-Corrente" });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"),
                columns: new[] { "Nome", "Titulo" },
                values: new object[] { "Yuzara", "a Tecelã do Destino" });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000003"),
                columns: new[] { "Arquetipo", "Nome", "Titulo" },
                values: new object[] { 14, "Thorvald", "o Arquiteto das Eras" });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000004"),
                column: "Titulo",
                value: null);

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000005"),
                column: "Titulo",
                value: null);

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000006"),
                column: "Titulo",
                value: null);

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000007"),
                columns: new[] { "Arquetipo", "Titulo" },
                values: new object[] { 17, null });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000008"),
                columns: new[] { "Arquetipo", "Titulo" },
                values: new object[] { 12, null });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000009"),
                columns: new[] { "Arquetipo", "Titulo" },
                values: new object[] { 13, null });

            migrationBuilder.InsertData(
                table: "HeroiConfigs",
                columns: new[] { "Id", "Arquetipo", "Nome", "RaridadeBase", "Tag", "Titulo" },
                values: new object[,]
                {
                    { new Guid("a2000000-0000-0000-0000-000000000001"), 1, "Lira", 3, "B1", "Flecha Dourada" },
                    { new Guid("a2000000-0000-0000-0000-000000000002"), 7, "Korin", 3, "B1", "Guardã do Bosque" },
                    { new Guid("a2000000-0000-0000-0000-000000000003"), 6, "Rinzi", 4, "B1", "Filha do Mercado" },
                    { new Guid("a2000000-0000-0000-0000-000000000004"), 2, "Selva", 3, "B1", "Filha da Raiz" },
                    { new Guid("a2000000-0000-0000-0000-000000000005"), 7, "Lune", 4, "B1", "Voz da Alcateia" },
                    { new Guid("a2000000-0000-0000-0000-000000000006"), 0, "Sera", 4, "B1", "Caçadora das Neves" },
                    { new Guid("a2000000-0000-0000-0000-000000000007"), 0, "Igara", 4, "B2", "Ruído do Vulcão" },
                    { new Guid("a2000000-0000-0000-0000-000000000008"), 0, "Varga", 4, "B2", "a Corrente Solta" },
                    { new Guid("a2000000-0000-0000-0000-000000000009"), 0, "Skaara", 4, "B2", "Fogo Desperto" },
                    { new Guid("a2000000-0000-0000-0000-000000000010"), 3, "Velara", 5, "B2", "a Sombra sem Nome" },
                    { new Guid("a2000000-0000-0000-0000-000000000011"), 7, "Nara", 4, "B2", "Maestrina Noturna" },
                    { new Guid("a2000000-0000-0000-0000-000000000012"), 2, "Elisse", 4, "B2", "a Ordem Perfeita" },
                    { new Guid("a2000000-0000-0000-0000-000000000013"), 4, "Draxa", 4, "B3", "a Fortaleza Viva" },
                    { new Guid("a2000000-0000-0000-0000-000000000014"), 3, "Kira", 4, "B3", "Lâmina do Crepúsculo" },
                    { new Guid("a2000000-0000-0000-0000-000000000015"), 2, "Marev", 4, "B3", "a Maré Eterna" },
                    { new Guid("a2000000-0000-0000-0000-000000000016"), 2, "Zara", 4, "B3", "a Bruxa do Vazio" },
                    { new Guid("a2000000-0000-0000-0000-000000000017"), 0, "Valdara", 5, "B3", "a Herança Negra" },
                    { new Guid("a2000000-0000-0000-0000-000000000018"), 2, "Lilith", 4, "B3", "Camareira do Caos" },
                    { new Guid("a2000000-0000-0000-0000-000000000019"), 0, "Zarael", 5, "B4", "a Acorrentada" },
                    { new Guid("a2000000-0000-0000-0000-000000000020"), 2, "Moira", 5, "B4", "a Ceifeira" },
                    { new Guid("a2000000-0000-0000-0000-000000000021"), 2, "Zephirael", 4, "B4", "a Tempestade Caída" },
                    { new Guid("a2000000-0000-0000-0000-000000000022"), 4, "Malachiel", 5, "B4", "a Muralha Quebrada" },
                    { new Guid("a2000000-0000-0000-0000-000000000023"), 2, "Vesper", 5, "B4", "o Abismo Vestido" },
                    { new Guid("a2000000-0000-0000-0000-000000000024"), 7, "Vrael", 5, "B4", "a Voz do Vácuo" },
                    { new Guid("a2000000-0000-0000-0000-000000000025"), 4, "Aelia", 5, "B5", "Sentinela do Limiar" },
                    { new Guid("a2000000-0000-0000-0000-000000000026"), 6, "Elyriel", 5, "B5", "a Última Canção" },
                    { new Guid("a2000000-0000-0000-0000-000000000027"), 0, "Seraphael", 5, "B5", "a Chama Corrompida" },
                    { new Guid("a2000000-0000-0000-0000-000000000028"), 5, "Lumira", 5, "B5", "Bênção da Alvorada" },
                    { new Guid("a2000000-0000-0000-0000-000000000029"), 0, "Aurael", 5, "B5", "o Punho do Éden" },
                    { new Guid("a2000000-0000-0000-0000-000000000030"), 3, "Nyx", 5, "B5", "Umbraveil" }
                });

            migrationBuilder.InsertData(
                table: "BiomHeroPools",
                columns: new[] { "Id", "BiomeId", "DropWeight", "EHeroPrincipal", "HeroiId", "Raridade" },
                values: new object[,]
                {
                    { new Guid("c2000000-0000-0000-0000-000000000001"), new Guid("b1000000-0000-0000-0000-000000000001"), 35, true, new Guid("a2000000-0000-0000-0000-000000000001"), 3 },
                    { new Guid("c2000000-0000-0000-0000-000000000002"), new Guid("b1000000-0000-0000-0000-000000000001"), 35, false, new Guid("a2000000-0000-0000-0000-000000000002"), 3 },
                    { new Guid("c2000000-0000-0000-0000-000000000003"), new Guid("b1000000-0000-0000-0000-000000000001"), 20, false, new Guid("a2000000-0000-0000-0000-000000000004"), 3 },
                    { new Guid("c2000000-0000-0000-0000-000000000004"), new Guid("b1000000-0000-0000-0000-000000000001"), 10, false, new Guid("a2000000-0000-0000-0000-000000000005"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000005"), new Guid("b1000000-0000-0000-0000-000000000002"), 30, true, new Guid("a2000000-0000-0000-0000-000000000007"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000006"), new Guid("b1000000-0000-0000-0000-000000000002"), 30, false, new Guid("a2000000-0000-0000-0000-000000000009"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000007"), new Guid("b1000000-0000-0000-0000-000000000002"), 25, false, new Guid("a2000000-0000-0000-0000-000000000011"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000008"), new Guid("b1000000-0000-0000-0000-000000000002"), 15, false, new Guid("a2000000-0000-0000-0000-000000000012"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000009"), new Guid("b1000000-0000-0000-0000-000000000003"), 35, true, new Guid("a2000000-0000-0000-0000-000000000014"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000010"), new Guid("b1000000-0000-0000-0000-000000000003"), 30, false, new Guid("a2000000-0000-0000-0000-000000000015"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000011"), new Guid("b1000000-0000-0000-0000-000000000003"), 20, false, new Guid("a2000000-0000-0000-0000-000000000018"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000012"), new Guid("b1000000-0000-0000-0000-000000000003"), 15, false, new Guid("a2000000-0000-0000-0000-000000000017"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000013"), new Guid("b1000000-0000-0000-0000-000000000004"), 30, true, new Guid("a2000000-0000-0000-0000-000000000020"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000014"), new Guid("b1000000-0000-0000-0000-000000000004"), 30, false, new Guid("a2000000-0000-0000-0000-000000000021"), 4 },
                    { new Guid("c2000000-0000-0000-0000-000000000015"), new Guid("b1000000-0000-0000-0000-000000000004"), 25, false, new Guid("a2000000-0000-0000-0000-000000000023"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000016"), new Guid("b1000000-0000-0000-0000-000000000004"), 15, false, new Guid("a2000000-0000-0000-0000-000000000022"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000017"), new Guid("b1000000-0000-0000-0000-000000000005"), 30, true, new Guid("a2000000-0000-0000-0000-000000000026"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000018"), new Guid("b1000000-0000-0000-0000-000000000005"), 25, false, new Guid("a2000000-0000-0000-0000-000000000027"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000019"), new Guid("b1000000-0000-0000-0000-000000000005"), 25, false, new Guid("a2000000-0000-0000-0000-000000000028"), 5 },
                    { new Guid("c2000000-0000-0000-0000-000000000020"), new Guid("b1000000-0000-0000-0000-000000000005"), 20, false, new Guid("a2000000-0000-0000-0000-000000000029"), 5 }
                });

            migrationBuilder.InsertData(
                table: "HeroiUnlockConfigs",
                columns: new[] { "HeroiId", "AndarMarco", "CondicaoDescricao", "QuantidadeFragmentos", "TipoUnlock" },
                values: new object[,]
                {
                    { new Guid("a2000000-0000-0000-0000-000000000001"), null, null, 20, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000002"), null, null, 20, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000003"), 8, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000004"), null, null, 25, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000005"), null, null, 30, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000006"), 20, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000007"), null, null, 35, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000008"), 26, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000009"), null, null, 38, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000010"), 36, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000011"), null, null, 42, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000012"), null, null, 45, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000013"), 51, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000014"), null, null, 50, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000015"), null, null, 52, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000016"), 60, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000017"), 68, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000018"), null, null, 56, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000019"), 76, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000020"), null, null, 58, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000021"), null, null, 60, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000022"), 88, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000023"), null, null, 62, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000024"), null, "Completar o Bioma 3 com 3 ou mais Bestiais na mesma party", null, 3 },
                    { new Guid("a2000000-0000-0000-0000-000000000025"), 101, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000026"), null, null, 65, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000027"), 108, null, null, 2 },
                    { new Guid("a2000000-0000-0000-0000-000000000028"), null, null, 68, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000029"), null, null, 70, 1 },
                    { new Guid("a2000000-0000-0000-0000-000000000030"), null, "Derrotar o chefe do Andar 120 sem perder nenhum herói na tentativa", null, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c2000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "HeroiUnlockConfigs",
                keyColumn: "HeroiId",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a2000000-0000-0000-0000-000000000030"));

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "Herois");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "HeroiConfigs");

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000001"),
                column: "AndarFim",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000002"),
                columns: new[] { "AndarFim", "AndarInicio" },
                values: new object[] { 25, 11 });

            migrationBuilder.UpdateData(
                table: "Biomas",
                keyColumn: "Id",
                keyValue: new Guid("b1000000-0000-0000-0000-000000000003"),
                columns: new[] { "AndarFim", "AndarInicio" },
                values: new object[] { 50, 26 });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"),
                column: "Nome",
                value: "Aldric, o Sem-Corrente");

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"),
                column: "Nome",
                value: "Yuzara, a Tecelã do Destino");

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000003"),
                columns: new[] { "Arquetipo", "Nome" },
                values: new object[] { 12, "Thorvald, o Arquiteto das Eras" });

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000007"),
                column: "Arquetipo",
                value: 15);

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000008"),
                column: "Arquetipo",
                value: 10);

            migrationBuilder.UpdateData(
                table: "HeroiConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000009"),
                column: "Arquetipo",
                value: 11);
        }
    }
}
