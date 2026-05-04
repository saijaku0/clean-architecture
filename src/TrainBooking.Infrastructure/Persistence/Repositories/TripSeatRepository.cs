using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class TripSeatRepository(AppDbContext appDbContext) : ITripSeatRepository
{
    public async Task<IReadOnlyList<TripSeat>> GetByTripIdAsync(
        Guid tripId,
        CancellationToken ct = default) =>
            await appDbContext.TripSeats.Where(t => t.TripId == tripId).ToListAsync(ct);

    /// <summary>
    /// Acquires update locks on the specified TripSeats for the duration of the current transaction.
    /// </summary>
    /// <remarks>
    /// Must be called inside an active database transaction (BeginTransactionAsync).
    /// Without a transaction, locks are released immediately after the SELECT completes.
    /// </remarks>
    public async Task<IReadOnlyList<TripSeat>> LockByIdsAsync(
        IReadOnlyCollection<Guid> tripSeatIds,
        CancellationToken ct = default)
    {
        Guid[] orderedIds = tripSeatIds.OrderBy(id => id).ToArray();

        string sql = $@"
            SELECT * FROM TripSeats WITH (UPDLOCK, ROWLOCK, HOLDLOCK)
            WHERE Id IN ({string.Join(",", orderedIds.Select((_, i) => $"{{{i}}}"))})
            ORDER BY Id";

        return await appDbContext.TripSeats
            .FromSqlRaw(sql, [.. orderedIds.Cast<object>()])
            .ToListAsync(ct);
    }

    public async Task AddRangeAsync(
        IEnumerable<TripSeat> tripSeats,
        CancellationToken ct = default) =>
            await appDbContext.AddRangeAsync(tripSeats, ct);
}
