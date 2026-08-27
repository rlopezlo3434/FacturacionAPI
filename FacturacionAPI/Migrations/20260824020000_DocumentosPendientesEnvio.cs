using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class DocumentosPendientesEnvio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentosPendientesEnvio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VentaOriginalId = table.Column<int>(type: "int", nullable: false),
                    ReemplazadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaProgramada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "PENDIENTE"),
                    Intentos = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NcVentaId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnviadoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosPendientesEnvio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosPendientesEnvio_Ventas_VentaOriginalId",
                        column: x => x.VentaOriginalId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosPendientesEnvio_Ventas_NcVentaId",
                        column: x => x.NcVentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosPendientesEnvio_Establishment_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPendientesEnvio_VentaOriginalId",
                table: "DocumentosPendientesEnvio",
                column: "VentaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPendientesEnvio_NcVentaId",
                table: "DocumentosPendientesEnvio",
                column: "NcVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPendientesEnvio_EstablishmentId",
                table: "DocumentosPendientesEnvio",
                column: "EstablishmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DocumentosPendientesEnvio");
        }
    }
}
