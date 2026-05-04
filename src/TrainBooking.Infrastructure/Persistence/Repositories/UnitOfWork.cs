using TrainBooking.Application.Abstractions.Repositories;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(AppDbContext dbContext)
    : IUnitOfWork
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<int> CommitAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);
}
