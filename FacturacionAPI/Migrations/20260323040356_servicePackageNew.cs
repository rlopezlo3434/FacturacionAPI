using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class servicePackageNew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicePackageId",
                table: "VehicleBudgetItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBudgetItems_ServicePackageId",
                table: "VehicleBudgetItems",
                column: "ServicePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleBudgetItems_ServicePackage_ServicePackageId",
                table: "VehicleBudgetItems",
                column: "ServicePackageId",
                principalTable: "ServicePackage",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleBudgetItems_ServicePackage_ServicePackageId",
                table: "VehicleBudgetItems");

            migrationBuilder.DropIndex(
                name: "IX_VehicleBudgetItems_ServicePackageId",
                table: "VehicleBudgetItems");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "VehicleBudgetItems");
        }
    }
}
