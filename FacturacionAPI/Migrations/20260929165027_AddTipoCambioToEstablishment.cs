using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddTipoCambioToEstablishment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TipoCambio",
                table: "Establishment",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoCambio",
                table: "Establishment");
        }
    }
}
