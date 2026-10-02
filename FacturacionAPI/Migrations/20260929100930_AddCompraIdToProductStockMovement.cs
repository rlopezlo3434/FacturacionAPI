using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddCompraIdToProductStockMovement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompraId",
                table: "ProductStockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovements_CompraId",
                table: "ProductStockMovements",
                column: "CompraId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStockMovements_Compras_CompraId",
                table: "ProductStockMovements",
                column: "CompraId",
                principalTable: "Compras",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductStockMovements_Compras_CompraId",
                table: "ProductStockMovements");

            migrationBuilder.DropIndex(
                name: "IX_ProductStockMovements_CompraId",
                table: "ProductStockMovements");

            migrationBuilder.DropColumn(
                name: "CompraId",
                table: "ProductStockMovements");
        }
    }
}
