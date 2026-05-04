using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class TrainRepository(AppDbContext dbContext) : ITrainRepository
{
    public async Task<Train?> GetByIdWithFullStructureAsync(
        Guid id,
        CancellationToken ct = default) =>
            await dbContext.Trains
                .Include(t => t.Wagons)
                .ThenInclude(w => w.Seats)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Train train, CancellationToken ct = default) =>
        await dbContext.Trains.AddAsync(train, ct);
}
