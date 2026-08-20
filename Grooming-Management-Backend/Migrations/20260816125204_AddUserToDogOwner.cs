using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToDogOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DogOwners",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogOwners_UserId",
                table: "DogOwners",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DogOwners_Users_UserId",
                table: "DogOwners",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DogOwners_Users_UserId",
                table: "DogOwners");

            migrationBuilder.DropIndex(
                name: "IX_DogOwners_UserId",
                table: "DogOwners");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DogOwners");
        }
    }
}
