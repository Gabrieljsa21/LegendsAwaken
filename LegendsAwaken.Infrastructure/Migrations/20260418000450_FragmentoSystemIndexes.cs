using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FragmentoSystemIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FragmentosProgresso_UsuarioId_Arquetipo",
                table: "FragmentosProgresso");

            migrationBuilder.DropIndex(
                name: "IX_FragmentosProgresso_UsuarioId_HeroiId",
                table: "FragmentosProgresso");

            migrationBuilder.InsertData(
                table: "BiomHeroPools",
                columns: new[] { "Id", "BiomeId", "DropWeight", "EHeroPrincipal", "HeroiId", "Raridade" },
                values: new object[] { new Guid("c1000000-0000-0000-0000-000000000008"), new Guid("b1000000-0000-0000-0000-000000000003"), 10, false, new Guid("a1000000-0000-0000-0000-000000000003"), 5 });

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_Arquetipo",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "Arquetipo" },
                unique: true,
                filter: "\"Arquetipo\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_HeroiId",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "HeroiId" },
                unique: true,
                filter: "\"HeroiId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FragmentosProgresso_UsuarioId_Arquetipo",
                table: "FragmentosProgresso");

            migrationBuilder.DropIndex(
                name: "IX_FragmentosProgresso_UsuarioId_HeroiId",
                table: "FragmentosProgresso");

            migrationBuilder.DeleteData(
                table: "BiomHeroPools",
                keyColumn: "Id",
                keyValue: new Guid("c1000000-0000-0000-0000-000000000008"));

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_Arquetipo",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "Arquetipo" });

            migrationBuilder.CreateIndex(
                name: "IX_FragmentosProgresso_UsuarioId_HeroiId",
                table: "FragmentosProgresso",
                columns: new[] { "UsuarioId", "HeroiId" });
        }
    }
}
