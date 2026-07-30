using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToGroomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Groomers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Groomers");
        }
    }
}
