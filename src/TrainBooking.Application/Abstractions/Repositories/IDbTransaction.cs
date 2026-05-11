namespace TrainBooking.Application.Abstractions.Repositories;

public interface IDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
