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

    public async Task<IReadOnlyList<TripSeat>> LockByIdsAsync(
        IReadOnlyCollection<Guid> tripSeatIds,
        CancellationToken ct = default) =>
            await appDbContext.TripSeats.Where(ts => tripSeatIds.Contains(ts.SeatId)).ToListAsync(ct);

    public async Task AddRangeAsync(
        IEnumerable<TripSeat> tripSeats,
        CancellationToken ct = default) =>
            await appDbContext.AddRangeAsync(tripSeats, ct);
}
