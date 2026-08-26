using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class MakeServiceBreedOptionalAddServiceToVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceBreedId",
                table: "Visits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Visits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ServiceId",
                table: "Visits",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Services_ServiceId",
                table: "Visits",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Services_ServiceId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_ServiceId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Visits");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceBreedId",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
