using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CidadeRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Construcao_Cidades_CidadeId",
                table: "Construcao");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonagemTrabalhador_Cidades_CidadeId",
                table: "PersonagemTrabalhador");

            migrationBuilder.DropColumn(
                name: "Profissao",
                table: "PersonagemTrabalhador");

            migrationBuilder.DropColumn(
                name: "TerminoTrabalho",
                table: "PersonagemTrabalhador");

            migrationBuilder.AlterColumn<Guid>(
                name: "CidadeId",
                table: "PersonagemTrabalhador",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CidadeId",
                table: "Construcao",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Recursos_Erva",
                table: "Cidades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaColeta",
                table: "Cidades",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Construcao_Cidades_CidadeId",
                table: "Construcao",
                column: "CidadeId",
                principalTable: "Cidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonagemTrabalhador_Cidades_CidadeId",
                table: "PersonagemTrabalhador",
                column: "CidadeId",
                principalTable: "Cidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Construcao_Cidades_CidadeId",
                table: "Construcao");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonagemTrabalhador_Cidades_CidadeId",
                table: "PersonagemTrabalhador");

            migrationBuilder.DropColumn(
                name: "Recursos_Erva",
                table: "Cidades");

            migrationBuilder.DropColumn(
                name: "UltimaColeta",
                table: "Cidades");

            migrationBuilder.AlterColumn<Guid>(
                name: "CidadeId",
                table: "PersonagemTrabalhador",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Profissao",
                table: "PersonagemTrabalhador",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminoTrabalho",
                table: "PersonagemTrabalhador",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CidadeId",
                table: "Construcao",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Construcao_Cidades_CidadeId",
                table: "Construcao",
                column: "CidadeId",
                principalTable: "Cidades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonagemTrabalhador_Cidades_CidadeId",
                table: "PersonagemTrabalhador",
                column: "CidadeId",
                principalTable: "Cidades",
                principalColumn: "Id");
        }
    }
}
