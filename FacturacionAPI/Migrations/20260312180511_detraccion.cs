using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class detraccion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Detraccion",
                table: "Ventas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DetraccionMonto",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DetraccionPorcentaje",
                table: "Ventas",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DetraccionTipo",
                table: "Ventas",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Detraccion",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DetraccionMonto",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DetraccionPorcentaje",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DetraccionTipo",
                table: "Ventas");
        }
    }
}
