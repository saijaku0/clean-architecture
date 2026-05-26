using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Reservations.Queries.GetMyReservations;
using TrainBooking.Application.Reservations.Queries.GetReservationById;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.Trips;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(AppDbContext dbContext) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default) =>
            await dbContext.Reservations
                .Include(r => r.ReservationSeats)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
    public async Task<int> CountActiveByUserOnTripAsync(
        Guid userId,
        Guid tripId,
        CancellationToken ct = default) =>
            await dbContext.Reservations
                .Where(r => r.UserId == userId
                    && r.TripId == tripId
                    && _activeStatuses.Contains(r.Status))
                .CountAsync(ct);
    public async Task<IReadOnlyList<Reservation>> GetExpiredPendingAsync(
        DateTime now,
        CancellationToken ct = default) =>
            await dbContext.Reservations
                .Include(r => r.ReservationSeats)
                .Where(r => r.Status == ReservationStatus.Pending
                    && r.ExpiresAt < now)
                .ToListAsync(ct);

    public async Task<IReadOnlyCollection<ReservationListDto>> GetUserReservationsAsync(
        Guid userId,
        CancellationToken ct = default) =>
            await dbContext.Reservations
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationListDto(
                    r.Id,
                    r.TripId,
                    r.TotalPrice,
                    r.Status.ToString(),
                    r.CreatedAt))
                .ToListAsync(ct);
    public async Task<ReservationDetailsDto?> GetReservationDetailsAsync(
        Guid reservationId,
        Guid userId,
        CancellationToken ct = default)
    {
        Reservation? reservation = await dbContext.Reservations
            .Include(r => r.ReservationSeats)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId, ct);

        if (reservation is null)
        {
            return null;
        }

        Trip? trip = await dbContext.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == reservation.TripId, ct);

        if (trip is null)
        {
            return null;
        }

        var seatIds = reservation.ReservationSeats
            .Select(rs => rs.TripSeatId)
            .ToList();

        return new ReservationDetailsDto(
            reservation.Id,
            reservation.TripId,
            trip.OriginStation,
            trip.DestinationStation,
            trip.DepartureTime,
            reservation.TotalPrice,
            reservation.Status.ToString(),
            seatIds
        );
    }
    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await dbContext.Reservations.AddAsync(reservation, ct);

    private static readonly ReservationStatus[] _activeStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed];
}
