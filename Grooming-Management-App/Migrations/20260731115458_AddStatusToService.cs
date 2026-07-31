using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "ActiveStatus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Groomers",
                newName: "ActiveStatus");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Services");

            migrationBuilder.RenameColumn(
                name: "ActiveStatus",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "ActiveStatus",
                table: "Groomers",
                newName: "Status");
        }
    }
}
