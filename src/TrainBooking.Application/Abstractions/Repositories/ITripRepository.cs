using TrainBooking.Domain.Trips;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task AddAsync(Trip Trip, CancellationToken ct = default);
}
