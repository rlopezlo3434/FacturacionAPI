using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class actualizaTablaWorkItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WorkOrderItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "VehicleBudgetItemId",
                table: "WorkOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItems_VehicleBudgetItemId",
                table: "WorkOrderItems",
                column: "VehicleBudgetItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderItems_VehicleBudgetItems_VehicleBudgetItemId",
                table: "WorkOrderItems",
                column: "VehicleBudgetItemId",
                principalTable: "VehicleBudgetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItems_VehicleBudgetItems_VehicleBudgetItemId",
                table: "WorkOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderItems_VehicleBudgetItemId",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WorkOrderItems");

            migrationBuilder.DropColumn(
                name: "VehicleBudgetItemId",
                table: "WorkOrderItems");
        }
    }
}
