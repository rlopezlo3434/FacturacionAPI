using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class internamiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryMasterItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Group = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMasterItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleIntakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    PickupAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    MileageKm = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleIntakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleIntakes_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleIntakes_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleIntakeInventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleIntakeId = table.Column<int>(type: "int", nullable: false),
                    InventoryMasterItemId = table.Column<int>(type: "int", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleIntakeInventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleIntakeInventoryItems_InventoryMasterItems_InventoryMasterItemId",
                        column: x => x.InventoryMasterItemId,
                        principalTable: "InventoryMasterItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleIntakeInventoryItems_VehicleIntakes_VehicleIntakeId",
                        column: x => x.VehicleIntakeId,
                        principalTable: "VehicleIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleIntakeInventoryItems_InventoryMasterItemId",
                table: "VehicleIntakeInventoryItems",
                column: "InventoryMasterItemId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleIntakeInventoryItems_VehicleIntakeId",
                table: "VehicleIntakeInventoryItems",
                column: "VehicleIntakeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleIntakes_ClientId",
                table: "VehicleIntakes",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleIntakes_VehicleId",
                table: "VehicleIntakes",
                column: "VehicleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleIntakeInventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryMasterItems");

            migrationBuilder.DropTable(
                name: "VehicleIntakes");
        }
    }
}
