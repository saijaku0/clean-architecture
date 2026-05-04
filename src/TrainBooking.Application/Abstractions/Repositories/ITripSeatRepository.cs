using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface ITripSeatRepository
{
    Task<IReadOnlyList<TripSeat>> GetByTripIdAsync(Guid tripId, CancellationToken ct = default);
    Task<IReadOnlyList<TripSeat>> LockByIdsAsync(IReadOnlyCollection<Guid> tripSeatIds, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<TripSeat> tripSeats, CancellationToken ct = default);
}
