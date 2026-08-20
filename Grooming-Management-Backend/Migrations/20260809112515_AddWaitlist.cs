using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Waitlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: false),
                    DogOwnerId = table.Column<int>(type: "int", nullable: false),
                    DogId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waitlists", x => x.Id);
                    table.CheckConstraint("CK_Waitlist_Priority_Range", "[Priority] >= 1 AND [Priority] <= 3");
                    table.ForeignKey(
                        name: "FK_Waitlists_DogOwners_DogOwnerId",
                        column: x => x.DogOwnerId,
                        principalTable: "DogOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Waitlists_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Waitlists_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Waitlists_DogId",
                table: "Waitlists",
                column: "DogId");

            migrationBuilder.CreateIndex(
                name: "IX_Waitlists_DogOwnerId",
                table: "Waitlists",
                column: "DogOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Waitlists_SalonId",
                table: "Waitlists",
                column: "SalonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Waitlists");
        }
    }
}
