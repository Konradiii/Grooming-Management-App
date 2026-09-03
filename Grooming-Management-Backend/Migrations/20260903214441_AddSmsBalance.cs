using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SmsIncluded",
                table: "Salons",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "SmsPurchased",
                table: "Salons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "SmsResetDate",
                table: "Salons",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmsIncluded",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "SmsPurchased",
                table: "Salons");

            migrationBuilder.DropColumn(
                name: "SmsResetDate",
                table: "Salons");
        }
    }
}
