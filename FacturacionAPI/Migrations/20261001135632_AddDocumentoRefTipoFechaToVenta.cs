using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddDocumentoRefTipoFechaToVenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DocumentoRefFecha",
                table: "Ventas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoRefTipo",
                table: "Ventas",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoRefFecha",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DocumentoRefTipo",
                table: "Ventas");
        }
    }
}
