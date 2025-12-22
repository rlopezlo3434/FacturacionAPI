using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class anulacionDOcumento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnulacionDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VentaId = table.Column<int>(type: "int", nullable: false),
                    CodigoUnico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnlacePdf = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnlaceXml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnlaceCdr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnulacionDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnulacionDocumento_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnulacionDocumento_VentaId",
                table: "AnulacionDocumento",
                column: "VentaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnulacionDocumento");
        }
    }
}
