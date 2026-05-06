using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInimigoCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inimigo_Andares_TorreAndarId",
                table: "Inimigo");

            migrationBuilder.DropIndex(
                name: "IX_Inimigo_TorreAndarId",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "Atributos_Agilidade",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "Atributos_Forca",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "Atributos_Inteligencia",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "TorreAndarId",
                table: "Inimigo");

            migrationBuilder.RenameColumn(
                name: "Nivel",
                table: "Inimigo",
                newName: "EChefe");

            migrationBuilder.RenameColumn(
                name: "Habilidades",
                table: "Inimigo",
                newName: "BiomaId");

            migrationBuilder.RenameColumn(
                name: "Atributos_Vitalidade",
                table: "Inimigo",
                newName: "AndarMinimo");

            migrationBuilder.RenameColumn(
                name: "Atributos_Percepcao",
                table: "Inimigo",
                newName: "AndarMaximo");

            migrationBuilder.AlterColumn<int>(
                name: "Tipo",
                table: "Inimigo",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ElementoAfinidade",
                table: "Inimigo",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElementoFraqueza",
                table: "Inimigo",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inimigo_BiomaId",
                table: "Inimigo",
                column: "BiomaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inimigo_Biomas_BiomaId",
                table: "Inimigo",
                column: "BiomaId",
                principalTable: "Biomas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inimigo_Biomas_BiomaId",
                table: "Inimigo");

            migrationBuilder.DropIndex(
                name: "IX_Inimigo_BiomaId",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "ElementoAfinidade",
                table: "Inimigo");

            migrationBuilder.DropColumn(
                name: "ElementoFraqueza",
                table: "Inimigo");

            migrationBuilder.RenameColumn(
                name: "EChefe",
                table: "Inimigo",
                newName: "Nivel");

            migrationBuilder.RenameColumn(
                name: "BiomaId",
                table: "Inimigo",
                newName: "Habilidades");

            migrationBuilder.RenameColumn(
                name: "AndarMinimo",
                table: "Inimigo",
                newName: "Atributos_Vitalidade");

            migrationBuilder.RenameColumn(
                name: "AndarMaximo",
                table: "Inimigo",
                newName: "Atributos_Percepcao");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Inimigo",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "Atributos_Agilidade",
                table: "Inimigo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Atributos_Forca",
                table: "Inimigo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Atributos_Inteligencia",
                table: "Inimigo",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TorreAndarId",
                table: "Inimigo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inimigo_TorreAndarId",
                table: "Inimigo",
                column: "TorreAndarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inimigo_Andares_TorreAndarId",
                table: "Inimigo",
                column: "TorreAndarId",
                principalTable: "Andares",
                principalColumn: "Id");
        }
    }
}
