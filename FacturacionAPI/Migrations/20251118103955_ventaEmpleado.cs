using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class ventaEmpleado : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ventaEmpleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    EmpleadoId = table.Column<int>(type: "int", nullable: false),
                    VentaItemId = table.Column<int>(type: "int", nullable: false),
                    productDefinitionId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventaEmpleados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ventaEmpleados_Employee_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventaEmpleados_ProductDefinition_productDefinitionId",
                        column: x => x.productDefinitionId,
                        principalTable: "ProductDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ventaEmpleados_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ventaEmpleados_EmpleadoId",
                table: "ventaEmpleados",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ventaEmpleados_productDefinitionId",
                table: "ventaEmpleados",
                column: "productDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ventaEmpleados_VentaId",
                table: "ventaEmpleados",
                column: "VentaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ventaEmpleados");
        }
    }
}
