using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class VehiculoDiagram : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleIntakeDiagram",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleIntakeId = table.Column<int>(type: "int", nullable: false),
                    MarkedImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleIntakeDiagram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleIntakeDiagram_VehicleIntakes_VehicleIntakeId",
                        column: x => x.VehicleIntakeId,
                        principalTable: "VehicleIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleIntakeDiagram_VehicleIntakeId",
                table: "VehicleIntakeDiagram",
                column: "VehicleIntakeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleIntakeDiagram");
        }
    }
}
