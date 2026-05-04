using TrainBooking.Domain.Trains;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface ITrainRepository
{
    Task<Train?> GetByIdWithFullStructureAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Train train, CancellationToken ct = default);
}
