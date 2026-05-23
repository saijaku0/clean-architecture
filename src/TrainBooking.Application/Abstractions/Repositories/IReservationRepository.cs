using TrainBooking.Application.Reservations.Queries.GetMyReservations;
using TrainBooking.Application.Reservations.Queries.GetReservationById;
using TrainBooking.Domain.Reservations;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> CountActiveByUserOnTripAsync(Guid userId, Guid tripId, CancellationToken ct = default);
    Task<IReadOnlyCollection<ReservationListDto>> GetUserReservationsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetExpiredPendingAsync(DateTime now, CancellationToken ct = default);
    Task<ReservationDetailsDto?> GetReservationDetailsAsync(Guid reservationId, Guid userId, CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
}
