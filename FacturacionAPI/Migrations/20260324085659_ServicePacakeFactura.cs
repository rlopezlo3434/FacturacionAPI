using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class ServicePacakeFactura : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicePackageId",
                table: "InvoicesItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesItem_ServicePackageId",
                table: "InvoicesItem",
                column: "ServicePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicesItem_ServicePackage_ServicePackageId",
                table: "InvoicesItem",
                column: "ServicePackageId",
                principalTable: "ServicePackage",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicesItem_ServicePackage_ServicePackageId",
                table: "InvoicesItem");

            migrationBuilder.DropIndex(
                name: "IX_InvoicesItem_ServicePackageId",
                table: "InvoicesItem");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "InvoicesItem");
        }
    }
}
