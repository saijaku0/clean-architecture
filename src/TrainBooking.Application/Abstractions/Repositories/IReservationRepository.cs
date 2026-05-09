using TrainBooking.Domain.Reservations;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> CountActiveByUserOnTripAsync(Guid userId, Guid tripId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetExpiredPendingAsync(DateTime now, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
}
