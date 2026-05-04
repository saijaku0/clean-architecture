using TrainBooking.Domain.Trips;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Trip trip, CancellationToken ct = default);
}
