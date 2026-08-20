using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class AddGroomerScheduleAndTimeSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroomerSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: false),
                    GroomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerSchedules_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerSchedules_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    SalonId = table.Column<int>(type: "int", nullable: false),
                    GroomerId = table.Column<int>(type: "int", nullable: false),
                    VisitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeSlots_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeSlots_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerSchedules_GroomerId",
                table: "GroomerSchedules",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerSchedules_SalonId",
                table: "GroomerSchedules",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_GroomerId",
                table: "TimeSlots",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_SalonId",
                table: "TimeSlots",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_VisitId",
                table: "TimeSlots",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroomerSchedules");

            migrationBuilder.DropTable(
                name: "TimeSlots");
        }
    }
}
