using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionAPI.Migrations
{
    public partial class CodePromotional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Promotion");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Promotion");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Promotion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Promotion");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Promotion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Promotion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
