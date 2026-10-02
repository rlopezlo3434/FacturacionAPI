using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddMonedaToVenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Ventas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "PEN");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Ventas");
        }
    }
}
