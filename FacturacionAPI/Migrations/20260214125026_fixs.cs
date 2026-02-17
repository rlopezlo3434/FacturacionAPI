using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class fixs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_VehicleBudgets_BudgetId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_BudgetId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "WorkOrders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_BudgetId",
                table: "WorkOrders",
                column: "BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_VehicleBudgets_BudgetId",
                table: "WorkOrders",
                column: "BudgetId",
                principalTable: "VehicleBudgets",
                principalColumn: "Id");
        }
    }
}
