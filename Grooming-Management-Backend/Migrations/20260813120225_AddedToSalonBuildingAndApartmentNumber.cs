using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddedToSalonBuildingAndApartmentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApartmentNumber",
                table: "Salons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Salons",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Salons",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxBookingDaysAhead",
                table: "Salons",
                type: "int",
                nullable: false,
                defaultValue: 90);

            migrationBuilder.AddColumn<int>(
                name: "MinBookingHoursAhead",
                table: "Salons",
                type: "int",
                nullable: false,
                defaultValue: 24);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Salons",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Salons",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApartmentNumber",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "MaxBookingDaysAhead",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "MinBookingHoursAhead",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Salons");
        }
    }
}
