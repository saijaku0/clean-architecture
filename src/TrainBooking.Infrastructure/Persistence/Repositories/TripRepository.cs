using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Trips;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository(AppDbContext dbContext) : ITripRepository
{
    public async Task<Trip?> GetByIdAsync(
        Guid Id,
        CancellationToken ct = default) =>
            await dbContext.Trips.FirstOrDefaultAsync(t => t.Id == Id, ct);

    public async Task AddAsync(
        Trip trip,
        CancellationToken ct = default) =>
            await dbContext.Trips.AddAsync(trip, ct);

}
