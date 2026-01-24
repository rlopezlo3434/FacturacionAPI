using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class budget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleIntakeId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsOfficial = table.Column<bool>(type: "bit", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleBudgets_VehicleIntakes_VehicleIntakeId",
                        column: x => x.VehicleIntakeId,
                        principalTable: "VehicleIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBudgetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleBudgetId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ServiceMasterId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBudgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleBudgetItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleBudgetItems_ServicesMasters_ServiceMasterId",
                        column: x => x.ServiceMasterId,
                        principalTable: "ServicesMasters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleBudgetItems_VehicleBudgets_VehicleBudgetId",
                        column: x => x.VehicleBudgetId,
                        principalTable: "VehicleBudgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBudgetItems_ProductId",
                table: "VehicleBudgetItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBudgetItems_ServiceMasterId",
                table: "VehicleBudgetItems",
                column: "ServiceMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBudgetItems_VehicleBudgetId",
                table: "VehicleBudgetItems",
                column: "VehicleBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleBudgets_VehicleIntakeId",
                table: "VehicleBudgets",
                column: "VehicleIntakeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleBudgetItems");

            migrationBuilder.DropTable(
                name: "VehicleBudgets");
        }
    }
}
