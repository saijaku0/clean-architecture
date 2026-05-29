using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Reservations.Queries.GetMyReservations;
using TrainBooking.Application.Reservations.Queries.GetReservationById;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.Trains;
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
        CancellationToken ct = default)
    {
        IQueryable<ReservationListDto> query = from r in dbContext.Reservations
                                               where r.UserId == userId
                                               join t in dbContext.Trips on r.TripId equals t.Id
                                               join train in dbContext.Trains on t.TrainId equals train.Id
                                               orderby r.CreatedAt descending
                                               select new ReservationListDto(
                                                   r.Id,
                                                   r.Status.ToString(),
                                                   r.ExpiresAt,
                                                   r.CreatedAt,
                                                   r.TripId,
                                                   train.Name,
                                                   t.OriginStation,
                                                   t.DestinationStation,
                                                   t.DepartureTime,
                                                   t.DepartureTime,
                                                   (from rs in r.ReservationSeats
                                                    join ts in dbContext.TripSeats on rs.TripSeatId equals ts.Id
                                                    join s in dbContext.Set<Seat>() on ts.SeatId equals s.Id
                                                    join w in dbContext.Set<Wagon>() on s.WagonId equals w.Id
                                                    select $"{w.Number}-{s.Number}").ToList(),
                                                   r.TotalPrice);
        return await query.ToListAsync(ct);
    }

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
