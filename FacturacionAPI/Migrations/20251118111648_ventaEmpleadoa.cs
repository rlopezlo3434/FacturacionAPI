using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class ventaEmpleadoa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventaEmpleados_ProductDefinition_productDefinitionId",
                table: "ventaEmpleados");

            migrationBuilder.DropColumn(
                name: "VentaItemId",
                table: "ventaEmpleados");

            migrationBuilder.RenameColumn(
                name: "productDefinitionId",
                table: "ventaEmpleados",
                newName: "ProductDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_ventaEmpleados_productDefinitionId",
                table: "ventaEmpleados",
                newName: "IX_ventaEmpleados_ProductDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ventaEmpleados_ProductDefinition_ProductDefinitionId",
                table: "ventaEmpleados",
                column: "ProductDefinitionId",
                principalTable: "ProductDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventaEmpleados_ProductDefinition_ProductDefinitionId",
                table: "ventaEmpleados");

            migrationBuilder.RenameColumn(
                name: "ProductDefinitionId",
                table: "ventaEmpleados",
                newName: "productDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_ventaEmpleados_ProductDefinitionId",
                table: "ventaEmpleados",
                newName: "IX_ventaEmpleados_productDefinitionId");

            migrationBuilder.AddColumn<int>(
                name: "VentaItemId",
                table: "ventaEmpleados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ventaEmpleados_ProductDefinition_productDefinitionId",
                table: "ventaEmpleados",
                column: "productDefinitionId",
                principalTable: "ProductDefinition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
