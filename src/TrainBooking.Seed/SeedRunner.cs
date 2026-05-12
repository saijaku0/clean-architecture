using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainBooking.Domain.Trains;
using TrainBooking.Domain.Trains.ValueObjects;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.TripSeats.Enums;
using TrainBooking.Infrastructure.Persistence;

namespace TrainBooking.Seed;

internal sealed class SeedRunner(
    AppDbContext dbContext,
    ILogger<SeedRunner> logger,
    DataFactory dataFactory,
    TimeProvider timeProvider)
{
    public async Task RunAsync(SeedAction action, CancellationToken ct = default)
    {
        logger.LogInformation("Starting seed operation: {Action}", action);

        switch (action)
        {
            case SeedAction.Seed:
                await SeedDataAsync(ct);
                break;
            case SeedAction.Full:
                await FullSeedDataAsync(ct);
                break;
            case SeedAction.Reset:
                await ResetReservationsAsync(ct);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(action), action, $"Unknown seed action: {action}");
        }

        logger.LogInformation("Seed action completed.");
    }

    private async Task ResetReservationsAsync(CancellationToken ct)
    {
        logger.LogInformation("Resetting reservations and restoring seat availability...");

        await dbContext.Reservations.ExecuteDeleteAsync(ct);

        int updatedRows = await dbContext.TripSeats
            .ExecuteUpdateAsync(
                s => s.SetProperty(ts => ts.Status, TripSeatStatus.Available),
                ct);

        logger.LogInformation(
            "Reservations cleared, {Count} TripSeats reset to Available.", updatedRows);
    }

    private async Task FullSeedDataAsync(CancellationToken ct)
    {
        logger.LogInformation("Performing full seed operation...");

        await DeleteAllDataAsync(ct);
        await SeedDataAsync(ct);
    }

    private async Task DeleteAllDataAsync(CancellationToken ct)
    {
        logger.LogInformation("Deleting all data from the database...");

        await dbContext.Reservations.ExecuteDeleteAsync(ct);
        await dbContext.Trips.ExecuteDeleteAsync(ct);
        await dbContext.Trains.ExecuteDeleteAsync(ct);
        await dbContext.TripSeats
            .ExecuteUpdateAsync(s => s.SetProperty(ts => ts.Status, TripSeatStatus.Available), ct);

        logger.LogInformation("All data deleted and TripSeats reset to Available.");
    }

    private async Task SeedDataAsync(CancellationToken ct)
    {
        logger.LogInformation("Seeding initial data...");

        Train train = dataFactory.CreateTrain(
            name: "Express Train",
            wagonCount: 2,
            seatsPerWagon: 4,
            wagonClass: SeatClass.SecondClass);

        Trip trip = dataFactory.CreateTrip(
            trainId: train.Id,
            origin: "Kyiv",
            destination: "Lviv",
            departure: timeProvider.GetUtcNow().UtcDateTime.AddDays(1),
            duration: TimeSpan.FromHours(8));

        List<TripSeat> tripSeats = dataFactory.CreateTripSeatsForTrain(
            tripId: trip.Id,
            train: train,
            basePrice: 100m);

        dbContext.Trains.Add(train);
        dbContext.Trips.Add(trip);
        dbContext.TripSeats.AddRange(tripSeats);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Initial data seeding completed.");
    }
}
