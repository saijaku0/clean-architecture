using Microsoft.EntityFrameworkCore.Storage;
using TrainBooking.Application.Abstractions.Repositories;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class UnitOfWork(AppDbContext dbContext)
    : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        IDbContextTransaction inner = await dbContext.Database.BeginTransactionAsync(ct);
        return new EfCoreDbTransaction(inner);
    }
}
