using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PupTrailsV3.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIntakeAndIncomeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPerLitter",
                table: "Intakes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Incomes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostPerLitter",
                table: "Intakes");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Incomes");
        }
    }
}
