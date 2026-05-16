using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Api.BackgroundServices;

internal sealed class ExpireReservationsService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpireReservationsService> logger,
    TimeProvider timeProvider) : BackgroundService
{
    private readonly TimeSpan _timePeriod = TimeSpan.FromMinutes(1);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(_timePeriod);
        while(await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessExpiredReservationsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process expired reservations");
            }
        }
    }

    private async Task ProcessExpiredReservationsAsync(
        CancellationToken stoppingToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        IReservationRepository reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
        ITripSeatRepository tripSeatRepository = scope.ServiceProvider.GetRequiredService<ITripSeatRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        IReadOnlyList<Reservation> expiredReservations = await reservationRepository.GetExpiredPendingAsync(now, stoppingToken);

        if (expiredReservations.Count == 0) return;

        logger.LogInformation("Expiring {Count} reservations", expiredReservations.Count);

        var tripSeatIds = expiredReservations
            .SelectMany(r => r.ReservationSeats)
            .Select(ri => ri.TripSeatId)
            .Distinct()
            .ToList();

        IReadOnlyCollection<TripSeat> tripSeats = await tripSeatRepository.GetByIdsAsync(tripSeatIds, stoppingToken);
        var tripSeatsById = tripSeats.ToDictionary(ts => ts.Id);

        int actuallyExpired = 0;
        int actuallyReleased = 0;

        foreach (Reservation reservation in expiredReservations)
        {
            Result expireResult = reservation.Expire(timeProvider);
            if (expireResult.IsFailure)
            {
                logger.LogWarning("Skipped reservation {ReservationId}: {ErrorCode} - {ErrorMessage}",
                    reservation.Id,
                    expireResult.Error?.Code,
                    expireResult.Error?.Message);

                continue;
            }

            actuallyExpired++;

            foreach (ReservationSeat rs in reservation.ReservationSeats)
                if (tripSeatsById.TryGetValue(rs.TripSeatId, out TripSeat? tripSeat))
                {
                    Result releaseResult = tripSeat.Release();
                    if (releaseResult.IsSuccess)
                        actuallyReleased++;
                    else
                        logger.LogWarning(
                            "Failed to release trip seat {TripSeatId} for reservation {ReservationId}",
                            rs.TripSeatId, reservation.Id);
                }
        }

        await unitOfWork.CommitAsync(stoppingToken);


        logger.LogInformation(
            "Expired {Expired}/{Found} reservations, released {Released} seats",
             actuallyExpired, expiredReservations.Count, actuallyReleased);
    }
}
