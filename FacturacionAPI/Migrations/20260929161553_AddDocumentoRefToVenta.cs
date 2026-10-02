using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddDocumentoRefToVenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentoRefNumero",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoRefSerie",
                table: "Ventas",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoRefNumero",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "DocumentoRefSerie",
                table: "Ventas");
        }
    }
}
