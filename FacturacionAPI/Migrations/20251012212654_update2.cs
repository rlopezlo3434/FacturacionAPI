using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_EstablishmentId",
                table: "Items",
                column: "EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Establishment_EstablishmentId",
                table: "Items",
                column: "EstablishmentId",
                principalTable: "Establishment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Establishment_EstablishmentId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_EstablishmentId",
                table: "Items");
        }
    }
}
