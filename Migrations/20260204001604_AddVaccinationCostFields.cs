using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PupTrailsV3.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationCostFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DAPPCost",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DistemperCost",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RabiesShotCost",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DAPPCost",
                table: "VetVisits");

            migrationBuilder.DropColumn(
                name: "DistemperCost",
                table: "VetVisits");

            migrationBuilder.DropColumn(
                name: "RabiesShotCost",
                table: "VetVisits");
        }
    }
}
