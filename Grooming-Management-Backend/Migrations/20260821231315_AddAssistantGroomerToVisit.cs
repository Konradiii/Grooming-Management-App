using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantGroomerToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssistantGroomerId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssistantGroomerId",
                table: "Visits",
                column: "AssistantGroomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Groomers_AssistantGroomerId",
                table: "Visits",
                column: "AssistantGroomerId",
                principalTable: "Groomers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Groomers_AssistantGroomerId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_AssistantGroomerId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "AssistantGroomerId",
                table: "Visits");
        }
    }
}
