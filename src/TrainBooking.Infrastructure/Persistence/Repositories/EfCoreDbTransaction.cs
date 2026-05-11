using Microsoft.EntityFrameworkCore.Storage;
using TrainBooking.Application.Abstractions.Repositories;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class EfCoreDbTransaction(IDbContextTransaction inner) : IDbTransaction
{
    private readonly IDbContextTransaction _inner = inner;

    public Task CommitAsync(CancellationToken ct = default) => _inner.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default) => _inner.RollbackAsync(ct);

    public ValueTask DisposeAsync() => _inner.DisposeAsync();
}
