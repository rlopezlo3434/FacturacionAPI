using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class Series : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SerieBoleta",
                table: "Establishment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerieFactura",
                table: "Establishment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenNubefact",
                table: "Establishment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "urlNubefact",
                table: "Establishment",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerieBoleta",
                table: "Establishment");

            migrationBuilder.DropColumn(
                name: "SerieFactura",
                table: "Establishment");

            migrationBuilder.DropColumn(
                name: "TokenNubefact",
                table: "Establishment");

            migrationBuilder.DropColumn(
                name: "urlNubefact",
                table: "Establishment");
        }
    }
}
