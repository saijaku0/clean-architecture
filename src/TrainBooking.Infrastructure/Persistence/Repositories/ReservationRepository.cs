using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
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
                    && ActiveStatuses.Contains(r.Status))
                .CountAsync(ct);
    public async Task<IReadOnlyList<Reservation>> GetExpiredPendingAsync(
        DateTime now,
        CancellationToken ct = default) =>
            await dbContext.Reservations
                .Include(r => r.ReservationSeats)
                .Where(r => r.Status == ReservationStatus.Pending
                    && r.ExpiresAt < now)
                .ToListAsync(ct);
    public async Task AddAsync(Reservation reservation, CancellationToken ct = default) =>
        await dbContext.Reservations.AddAsync(reservation, ct);

    private static readonly ReservationStatus[] ActiveStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed];
}
