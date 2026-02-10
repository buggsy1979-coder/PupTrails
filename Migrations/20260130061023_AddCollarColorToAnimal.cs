using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PupTrailsV3.Migrations
{
    /// <inheritdoc />
    public partial class AddCollarColorToAnimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollarColor",
                table: "Animals",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollarColor",
                table: "Animals");
        }
    }
}
