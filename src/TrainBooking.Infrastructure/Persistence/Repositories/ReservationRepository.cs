using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Reservations.Queries.GetMyReservations;
using TrainBooking.Application.Reservations.Queries.GetReservationById;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;

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
        var query = from r in dbContext.Reservations.AsNoTracking()
                    where r.Id == reservationId && r.UserId == userId
                    join t in dbContext.Trips.AsNoTracking() on r.TripId equals t.Id
                    select new { Reservation = r, Trip = t };

        var data = await query.FirstOrDefaultAsync(ct);

        if (data is null)
        {
            return null;
        }

        var seatIds = data.Reservation.ReservationSeats
            .Select(rs => rs.TripSeatId)
            .ToList();

        return new ReservationDetailsDto(
            data.Reservation.Id,
            data.Reservation.TripId,
            data.Trip.OriginStation,
            data.Trip.DestinationStation,
            data.Trip.DepartureTime,
            data.Reservation.TotalPrice,
            data.Reservation.Status.ToString(),
            seatIds
        );
    }
    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await dbContext.Reservations.AddAsync(reservation, ct);

    private static readonly ReservationStatus[] _activeStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed];
}
