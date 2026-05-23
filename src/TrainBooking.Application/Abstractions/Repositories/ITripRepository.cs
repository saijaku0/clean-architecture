using TrainBooking.Application.Common.Pagination;
using TrainBooking.Application.Trips.GetTripById;
using TrainBooking.Application.Trips.GetTripList;
using TrainBooking.Domain.Trips;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface ITripRepository
{
    Task<Trip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TripDetailsDto?> GetTripDetailsAsync(Guid tripId, CancellationToken ct);
    Task<PagedResult<TripListItemDto>> GetListAsync(
        string? origin,
        string? destination,
        DateTime? departureDate,
        int pageNumber,
        int pageSize,
        DateTime now,
        CancellationToken ct = default);
}
