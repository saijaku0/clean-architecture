using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Common.Pagination;

namespace TrainBooking.Application.Trips.GetTripList;

public sealed record GetTripListQuery(
    string? Origin = null,
    string? Destination = null,
    DateTime? DepartureDate = null,
    int PageNumber = 1,
    int PageSize = 10) : IQuery<PagedResult<TripListItemDto>>;
