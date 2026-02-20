using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class Invoicesss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VehicleBudgetItemId",
                table: "InvoicesItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_ProductId",
                table: "InvoicesItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_ServiceMasterId",
                table: "InvoicesItem",
                column: "ServiceMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_VehicleBudgetItemId",
                table: "InvoicesItem",
                column: "VehicleBudgetItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItem_Products_ProductId",
                table: "InvoicesItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItem_ServicesMasters_ServiceMasterId",
                table: "InvoicesItem",
                column: "ServiceMasterId",
                principalTable: "ServicesMasters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItem_VehicleBudgetItems_VehicleBudgetItemId",
                table: "InvoicesItem",
                column: "VehicleBudgetItemId",
                principalTable: "VehicleBudgetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItem_Products_ProductId",
                table: "InvoicesItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItem_ServicesMasters_ServiceMasterId",
                table: "InvoicesItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItem_VehicleBudgetItems_VehicleBudgetItemId",
                table: "InvoicesItem");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesItem_ProductId",
                table: "InvoicesItem");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesItem_ServiceMasterId",
                table: "InvoicesItem");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesItem_VehicleBudgetItemId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "VehicleBudgetItemId",
                table: "InvoicesItem");
        }
    }
}
