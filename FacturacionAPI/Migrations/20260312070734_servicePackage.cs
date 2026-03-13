using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class servicePackage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicePackage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackageItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicePackageId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    ServiceMasterId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackageItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackageItem_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicePackageItem_ServicePackage_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicePackageItem_ServicesMasters_ServiceMasterId",
                        column: x => x.ServiceMasterId,
                        principalTable: "ServicesMasters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageItem_ProductId",
                table: "ServicePackageItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageItem_ServiceMasterId",
                table: "ServicePackageItem",
                column: "ServiceMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackageItem_ServicePackageId",
                table: "ServicePackageItem",
                column: "ServicePackageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePackageItem");

            migrationBuilder.DropTable(
                name: "ServicePackage");
        }
    }
}
