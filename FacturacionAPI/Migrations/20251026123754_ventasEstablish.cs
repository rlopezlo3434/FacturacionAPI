using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class ventasEstablish : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstablishmentId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_EstablishmentId",
                table: "Ventas",
                column: "EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Establishment_EstablishmentId",
                table: "Ventas",
                column: "EstablishmentId",
                principalTable: "Establishment",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Establishment_EstablishmentId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_EstablishmentId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "EstablishmentId",
                table: "Ventas");
        }
    }
}
