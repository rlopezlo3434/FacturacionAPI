using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddConIgvToCompra : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConIgv",
                table: "Compras",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConIgv",
                table: "Compras");
        }
    }
}
