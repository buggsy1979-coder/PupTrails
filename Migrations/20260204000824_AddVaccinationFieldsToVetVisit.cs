using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PupTrailsV3.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationFieldsToVetVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DAPPDate",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DistemperDate",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RabiesShotDate",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaccinationsGiven",
                table: "VetVisits",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DAPPDate",
                table: "VetVisits");

            migrationBuilder.DropColumn(
                name: "DistemperDate",
                table: "VetVisits");

            migrationBuilder.DropColumn(
                name: "RabiesShotDate",
                table: "VetVisits");

            migrationBuilder.DropColumn(
                name: "VaccinationsGiven",
                table: "VetVisits");
        }
    }
}
