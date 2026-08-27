using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Grooming_Management_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BuildingNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ApartmentNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MinBookingHoursAhead = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    MaxBookingDaysAhead = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    SubscriptionValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    SubscriptionStatus = table.Column<int>(type: "integer", nullable: false),
                    ProviderCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderSubscriptionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProviderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SalonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    ActiveStatus = table.Column<int>(type: "integer", nullable: false),
                    RequiresPasswordChange = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBreeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    BreedId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBreeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBreeds_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceBreeds_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceBreeds_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DogOwners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogOwners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DogOwners_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DogOwners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Groomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SettlementType = table.Column<int>(type: "integer", nullable: false),
                    SettlementRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActiveStatus = table.Column<int>(type: "integer", nullable: false),
                    CanSeeAllVisits = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanCreateVisits = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groomers_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TokenHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AgeInMonths = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    BreedId = table.Column<int>(type: "integer", nullable: false),
                    DogOwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dogs_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dogs_DogOwners_DogOwnerId",
                        column: x => x.DogOwnerId,
                        principalTable: "DogOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dogs_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    GroomerId = table.Column<int>(type: "integer", nullable: false)
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
                name: "GroomerTimeOffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    GroomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerTimeOffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerTimeOffs_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerTimeOffs_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Blacklists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    DogOwnerId = table.Column<int>(type: "integer", nullable: false),
                    DogId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blacklists_DogOwners_DogOwnerId",
                        column: x => x.DogOwnerId,
                        principalTable: "DogOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blacklists_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Blacklists_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    ProposedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AssistantGroomerId = table.Column<int>(type: "integer", nullable: true),
                    SettlementType = table.Column<int>(type: "integer", nullable: false),
                    SettlementRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    DogId = table.Column<int>(type: "integer", nullable: false),
                    DogOwnerId = table.Column<int>(type: "integer", nullable: false),
                    GroomerId = table.Column<int>(type: "integer", nullable: false),
                    ServiceBreedId = table.Column<int>(type: "integer", nullable: true),
                    ServiceId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_DogOwners_DogOwnerId",
                        column: x => x.DogOwnerId,
                        principalTable: "DogOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Groomers_AssistantGroomerId",
                        column: x => x.AssistantGroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_ServiceBreeds_ServiceBreedId",
                        column: x => x.ServiceBreedId,
                        principalTable: "ServiceBreeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Waitlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    DogOwnerId = table.Column<int>(type: "integer", nullable: false),
                    DogId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waitlists", x => x.Id);
                    table.CheckConstraint("CK_Waitlist_Priority_Range", "\"Priority\" >= 1 AND \"Priority\" <= 3");
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

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MessageText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SalonId = table.Column<int>(type: "integer", nullable: false),
                    VisitId = table.Column<int>(type: "integer", nullable: false),
                    DogOwnerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_DogOwners_DogOwnerId",
                        column: x => x.DogOwnerId,
                        principalTable: "DogOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Salons_SalonId",
                        column: x => x.SalonId,
                        principalTable: "Salons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blacklists_DogId",
                table: "Blacklists",
                column: "DogId");

            migrationBuilder.CreateIndex(
                name: "IX_Blacklists_DogOwnerId",
                table: "Blacklists",
                column: "DogOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Blacklists_SalonId",
                table: "Blacklists",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_DogOwners_SalonId",
                table: "DogOwners",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_DogOwners_UserId",
                table: "DogOwners",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_BreedId",
                table: "Dogs",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_DogOwnerId",
                table: "Dogs",
                column: "DogOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_SalonId",
                table: "Dogs",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_SalonId",
                table: "Groomers",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_UserId",
                table: "Groomers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroomerSchedules_GroomerId",
                table: "GroomerSchedules",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerSchedules_SalonId",
                table: "GroomerSchedules",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerTimeOffs_GroomerId",
                table: "GroomerTimeOffs",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerTimeOffs_SalonId",
                table: "GroomerTimeOffs",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DogOwnerId",
                table: "Notifications",
                column: "DogOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SalonId",
                table: "Notifications",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_VisitId",
                table: "Notifications",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderId",
                table: "Payments",
                column: "ProviderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SalonId",
                table: "Payments",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Salons_ProviderCustomerId",
                table: "Salons",
                column: "ProviderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBreeds_BreedId",
                table: "ServiceBreeds",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBreeds_SalonId",
                table: "ServiceBreeds",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBreeds_ServiceId",
                table: "ServiceBreeds",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_SalonId",
                table: "Services",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SalonId",
                table: "Users",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssistantGroomerId",
                table: "Visits",
                column: "AssistantGroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DogId",
                table: "Visits",
                column: "DogId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DogOwnerId",
                table: "Visits",
                column: "DogOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_GroomerId",
                table: "Visits",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SalonId",
                table: "Visits",
                column: "SalonId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ServiceBreedId",
                table: "Visits",
                column: "ServiceBreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ServiceId",
                table: "Visits",
                column: "ServiceId");

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
                name: "Blacklists");

            migrationBuilder.DropTable(
                name: "GroomerSchedules");

            migrationBuilder.DropTable(
                name: "GroomerTimeOffs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Waitlists");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "Dogs");

            migrationBuilder.DropTable(
                name: "Groomers");

            migrationBuilder.DropTable(
                name: "ServiceBreeds");

            migrationBuilder.DropTable(
                name: "DogOwners");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Salons");
        }
    }
}
