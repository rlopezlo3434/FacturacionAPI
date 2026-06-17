using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class TarjetaPorHijo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildrenClientId",
                table: "VisitaClientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TarjetaCicloActual",
                table: "ChildrenClient",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChildrenClientId",
                table: "VisitaClientes");

            migrationBuilder.DropColumn(
                name: "TarjetaCicloActual",
                table: "ChildrenClient");
        }
    }
}
