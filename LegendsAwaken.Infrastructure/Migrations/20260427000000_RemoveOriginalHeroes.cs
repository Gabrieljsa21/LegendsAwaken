using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    public partial class RemoveOriginalHeroes : Migration
    {
        private static readonly string[] OriginalHeroIds =
        [
            "a1000000-0000-0000-0000-000000000001",
            "a1000000-0000-0000-0000-000000000002",
            "a1000000-0000-0000-0000-000000000003",
            "a1000000-0000-0000-0000-000000000004",
            "a1000000-0000-0000-0000-000000000005",
            "a1000000-0000-0000-0000-000000000006",
            "a1000000-0000-0000-0000-000000000007",
            "a1000000-0000-0000-0000-000000000008",
            "a1000000-0000-0000-0000-000000000009",
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var ids = string.Join(",", Array.ConvertAll(OriginalHeroIds, id => $"'{id}'"));

            // Children first to satisfy FK constraints
            migrationBuilder.Sql($@"DELETE FROM ""BiomHeroPools"" WHERE ""HeroiId"" IN ({ids})");
            migrationBuilder.Sql($@"DELETE FROM ""HeroiUnlockConfigs"" WHERE ""HeroiId"" IN ({ids})");
            migrationBuilder.Sql($@"DELETE FROM ""HeroiConfigs"" WHERE ""Id"" IN ({ids})");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "HeroiConfigs",
                columns: new[] { "Id", "Arquetipo", "Nome", "RaridadeBase", "Tag", "Titulo" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 0,  "Aldric",   5, null, "o Sem-Corrente"       },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 2,  "Yuzara",   5, null, "a Tecelã do Destino"  },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), 12, "Thorvald", 5, null, "o Arquiteto das Eras" },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 1,  "Kaen",     4, null, null                  },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), 3,  "Nyra",     4, null, null                  },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), 4,  "Seraph",   4, null, null                  },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), 15, "Mira",     4, null, null                  },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), 10, "Grom",     4, null, null                  },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), 11, "Hana",     4, null, null                  },
                });

            migrationBuilder.InsertData(
                table: "BiomHeroPools",
                columns: new[] { "Id", "BiomeId", "DropWeight", "EHeroPrincipal", "HeroiId", "Raridade" },
                values: new object[,]
                {
                    { new Guid("c1000000-0000-0000-0000-000000000001"), new Guid("b1000000-0000-0000-0000-000000000001"), 30, true,  new Guid("a1000000-0000-0000-0000-000000000004"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000002"), new Guid("b1000000-0000-0000-0000-000000000001"), 70, false, new Guid("a1000000-0000-0000-0000-000000000009"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000003"), new Guid("b1000000-0000-0000-0000-000000000002"), 30, true,  new Guid("a1000000-0000-0000-0000-000000000006"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000004"), new Guid("b1000000-0000-0000-0000-000000000002"), 70, false, new Guid("a1000000-0000-0000-0000-000000000005"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000005"), new Guid("b1000000-0000-0000-0000-000000000003"), 20, true,  new Guid("a1000000-0000-0000-0000-000000000001"), 5 },
                    { new Guid("c1000000-0000-0000-0000-000000000006"), new Guid("b1000000-0000-0000-0000-000000000003"), 45, false, new Guid("a1000000-0000-0000-0000-000000000007"), 4 },
                    { new Guid("c1000000-0000-0000-0000-000000000007"), new Guid("b1000000-0000-0000-0000-000000000003"), 35, false, new Guid("a1000000-0000-0000-0000-000000000008"), 4 },
                });

            migrationBuilder.InsertData(
                table: "HeroiUnlockConfigs",
                columns: new[] { "HeroiId", "AndarMarco", "CondicaoDescricao", "QuantidadeFragmentos", "TipoUnlock" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 30,   null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 60,   null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), null, null, 60,   1 },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 10,   null, null, 2 },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), null, "Completar o andar 15 com a party completa sem nenhum herói ser derrotado", null, 3 },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), null, null, 40,   1 },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), null, null, 35,   1 },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), null, null, 30,   1 },
                    { new Guid("a1000000-0000-0000-0000-000000000009"), null, "Ter pelo menos 3 heróis com Humor >= 80 na cidade ao mesmo tempo", null, 3 },
                });
        }
    }
}
