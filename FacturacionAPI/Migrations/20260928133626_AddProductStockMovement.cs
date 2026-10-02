using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class AddProductStockMovement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductStockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DocumentoTipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentoSerie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentoNumero = table.Column<int>(type: "int", nullable: false),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductStockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductStockMovements_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovements_ProductId",
                table: "ProductStockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockMovements_VentaId",
                table: "ProductStockMovements",
                column: "VentaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductStockMovements");
        }
    }
}
