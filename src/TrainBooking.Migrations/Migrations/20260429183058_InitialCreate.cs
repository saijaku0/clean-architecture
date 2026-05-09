using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainBooking.Migrations.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Reservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                TripDepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Trains",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trains", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Trips",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OriginStation = table.Column<string>(type: "nvarchar(155)", maxLength: 155, nullable: false),
                DestinationStation = table.Column<string>(type: "nvarchar(155)", maxLength: 155, nullable: false),
                DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trips", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TripSeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TripSeats", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Auth0Sub = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ReservationSeats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TripSeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReservationSeats", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReservationSeats_Reservations_ReservationId",
                    column: x => x.ReservationId,
                    principalTable: "Reservations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Wagons",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TrainId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Number = table.Column<int>(type: "int", nullable: false),
                Class = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Wagons", x => x.Id);
                table.ForeignKey(
                    name: "FK_Wagons_Trains_TrainId",
                    column: x => x.TrainId,
                    principalTable: "Trains",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Seats",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WagonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Number = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Seats", x => x.Id);
                table.ForeignKey(
                    name: "FK_Seats_Wagons_WagonId",
                    column: x => x.WagonId,
                    principalTable: "Wagons",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_Status_ExpiresAt",
            table: "Reservations",
            columns: new[] { "Status", "ExpiresAt" },
            filter: "[Status] = 'Pending'");

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_UserId_TripId",
            table: "Reservations",
            columns: new[] { "UserId", "TripId" });

        migrationBuilder.CreateIndex(
            name: "IX_ReservationSeats_ReservationId",
            table: "ReservationSeats",
            column: "ReservationId");

        migrationBuilder.CreateIndex(
            name: "IX_ReservationSeats_TripSeatId",
            table: "ReservationSeats",
            column: "TripSeatId");

        migrationBuilder.CreateIndex(
            name: "IX_Seats_WagonId",
            table: "Seats",
            column: "WagonId");

        migrationBuilder.CreateIndex(
            name: "IX_Trips_OriginStation_DestinationStation_DepartureTime",
            table: "Trips",
            columns: new[] { "OriginStation", "DestinationStation", "DepartureTime" });

        migrationBuilder.CreateIndex(
            name: "IX_TripSeats_TripId_Available_Filtered",
            table: "TripSeats",
            columns: new[] { "TripId", "Status" },
            filter: "[Status] = 'Available'");

        migrationBuilder.CreateIndex(
            name: "IX_TripSeats_TripId_SeatId",
            table: "TripSeats",
            columns: new[] { "TripId", "SeatId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Auth0Sub",
            table: "Users",
            column: "Auth0Sub",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Wagons_TrainId",
            table: "Wagons",
            column: "TrainId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ReservationSeats");

        migrationBuilder.DropTable(
            name: "Seats");

        migrationBuilder.DropTable(
            name: "Trips");

        migrationBuilder.DropTable(
            name: "TripSeats");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Reservations");

        migrationBuilder.DropTable(
            name: "Wagons");

        migrationBuilder.DropTable(
            name: "Trains");
    }
}
