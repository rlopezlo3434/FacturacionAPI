using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class InvoiceBudgetAndPackageSummary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Invoice.BudgetId
            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BudgetId",
                table: "Invoices",
                column: "BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_VehicleBudgets_BudgetId",
                table: "Invoices",
                column: "BudgetId",
                principalTable: "VehicleBudgets",
                principalColumn: "Id");

            // InvoiceItem.IsPackageSummary
            migrationBuilder.AddColumn<bool>(
                name: "IsPackageSummary",
                table: "InvoicesItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // InvoiceItem.PackageDescription
            migrationBuilder.AddColumn<string>(
                name: "PackageDescription",
                table: "InvoicesItem",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_VehicleBudgets_BudgetId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BudgetId",
                table: "Invoices");

            migrationBuilder.DropColumn(name: "BudgetId", table: "Invoices");
            migrationBuilder.DropColumn(name: "IsPackageSummary", table: "InvoicesItem");
            migrationBuilder.DropColumn(name: "PackageDescription", table: "InvoicesItem");
        }
    }
}
