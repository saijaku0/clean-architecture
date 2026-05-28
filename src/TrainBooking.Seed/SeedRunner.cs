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

        // Физические поезда — переиспользуются между несколькими рейсами
        Train expressTrain = dataFactory.CreateTrain(
            name: "Express Train",
            wagonCount: 2,
            seatsPerWagon: 4,
            wagonClass: SeatClass.SecondClass);

        Train intercityTrain = dataFactory.CreateTrain(
            name: "Intercity 125",
            wagonCount: 4,
            seatsPerWagon: 6,
            wagonClass: SeatClass.FirstClass);

        Train nightExpress = dataFactory.CreateTrain(
            name: "Night Express",
            wagonCount: 3,
            seatsPerWagon: 4,
            wagonClass: SeatClass.SecondClass);

        dbContext.Trains.AddRange(expressTrain, intercityTrain, nightExpress);

        // Шаблоны расписания — каждый маршрут ходит раз в день в течение окна сидинга
        (Train Train, string Origin, string Destination, int Hour, TimeSpan Duration, decimal BasePrice)[] schedule =
        [
            (expressTrain,   "Kyiv", "Lviv",    8,  TimeSpan.FromHours(8), 100m),
        (intercityTrain, "Kyiv", "Odesa",   12, TimeSpan.FromHours(6), 150m),
        (expressTrain,   "Lviv", "Kyiv",    17, TimeSpan.FromHours(8), 100m),
        (nightExpress,   "Kyiv", "Kharkiv", 22, TimeSpan.FromHours(7), 80m),
    ];

        var trips = new List<Trip>();
        var tripSeats = new List<TripSeat>();

        DateTime startDate = timeProvider.GetUtcNow().UtcDateTime.Date.AddDays(1);
        const int daysToSeed = 7;

        for (int dayOffset = 0; dayOffset < daysToSeed; dayOffset++)
        {
            foreach (var route in schedule)
            {
                DateTime departure = startDate.AddDays(dayOffset).AddHours(route.Hour);

                Trip trip = dataFactory.CreateTrip(
                    trainId: route.Train.Id,
                    origin: route.Origin,
                    destination: route.Destination,
                    departure: departure,
                    duration: route.Duration);

                trips.Add(trip);
                tripSeats.AddRange(dataFactory.CreateTripSeatsForTrain(
                    tripId: trip.Id,
                    train: route.Train,
                    basePrice: route.BasePrice));
            }
        }

        dbContext.Trips.AddRange(trips);
        dbContext.TripSeats.AddRange(tripSeats);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Seeded {TripCount} trips across {Days} days, {SeatCount} seats.",
            trips.Count, daysToSeed, tripSeats.Count);
    }
}
