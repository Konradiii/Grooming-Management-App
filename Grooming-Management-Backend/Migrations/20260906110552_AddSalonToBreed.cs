using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddSalonToBreed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalonId",
                table: "Breeds",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_SalonId",
                table: "Breeds",
                column: "SalonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Breeds_Salons_SalonId",
                table: "Breeds",
                column: "SalonId",
                principalTable: "Salons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Breeds_Salons_SalonId",
                table: "Breeds");

            migrationBuilder.DropIndex(
                name: "IX_Breeds_SalonId",
                table: "Breeds");

            migrationBuilder.DropColumn(
                name: "SalonId",
                table: "Breeds");
        }
    }
}
