using Microsoft.EntityFrameworkCore.Migrations;

namespace PupTrailsV3.Migrations
{
    public partial class AddMachineIdToLicense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MachineId",
                table: "Licenses",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Licenses");
        }
    }
}
