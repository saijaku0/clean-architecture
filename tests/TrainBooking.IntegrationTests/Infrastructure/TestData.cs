using Microsoft.EntityFrameworkCore;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Trains;
using TrainBooking.Domain.Trains.ValueObjects;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.Users;
using TrainBooking.Infrastructure.Persistence;

namespace TrainBooking.IntegrationTests.Infrastructure;

public sealed record ReservationScenarioData(
    Guid UserId,
    Guid TripId,
    IReadOnlyList<Guid> TripSeatIds,
    DateTime TripDepartureTime);

public static class TestData
{
    public static async Task<ReservationScenarioData> SeedReservationScenarioAsync(
        DatabaseFixture fixture,
        int seatsCount = 4,
        decimal seatPrice = 100.00m,
        CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = fixture.CreateDbContext();

        var train = Train.Create("Test Train");

        const int wagonNumber = 1;
        train.AddWagon(wagonNumber, SeatClass.SecondClass);

        for (int seatNumber = 1; seatNumber <= seatsCount; seatNumber++)
            train.AddSeatToWagon(wagonNumber, seatNumber);

        DateTime departure = DateTime.UtcNow.AddDays(7);
        DateTime arrival = departure.AddHours(8);

        Result<Trip> tripResult = Trip.Create(
            train.Id,
            "Kyiv",
            "Lviv",
            departure,
            arrival,
            TimeProvider.System);

        if (tripResult.IsFailure)
            throw new InvalidOperationException(
                $"Failed to create test Trip: {tripResult.Error?.Message}");

        Trip trip = tripResult.Value;

        Seat[] seats = train.Wagons.SelectMany(w => w.Seats).ToArray();
        TripSeat[] tripSeats = [.. seats.Select(seat => TripSeat.Create(trip.Id, seat.Id, seatPrice))];

        var user = User.Create(
            auth0Sub: $"auth0|test-{Guid.NewGuid()}",
            email: "test@example.com",
            fullName: "Test User");

        dbContext.Trains.Add(train);
        dbContext.Trips.Add(trip);
        dbContext.TripSeats.AddRange(tripSeats);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ReservationScenarioData(
            UserId: user.Id,
            TripId: trip.Id,
            TripSeatIds: tripSeats.Select(ts => ts.Id).ToArray(),
            TripDepartureTime: departure);
    }

    public static async Task CleanDatabaseAsync(DatabaseFixture fixture, CancellationToken cancellationToken = default)
    {
        await using AppDbContext dbContext = fixture.CreateDbContext();

        await dbContext.Reservations.ExecuteDeleteAsync(cancellationToken);
        await dbContext.TripSeats.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Trips.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Trains.ExecuteDeleteAsync(cancellationToken);
        await dbContext.Users.ExecuteDeleteAsync(cancellationToken);
    }
}
