using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Common.Pagination;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Trips.GetTripList;

internal sealed class GetTripListQueryHandler(
    ITripRepository tripRepository,
    TimeProvider timeProvider) : IQueryHandler<GetTripListQuery, PagedResult<TripListItemDto>>
{
    public async Task<Result<PagedResult<TripListItemDto>>> Handle(
        GetTripListQuery request,
        CancellationToken ct)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        PagedResult<TripListItemDto> result = await tripRepository.GetListAsync(
            request.Origin,
            request.Destination,
            request.DepartureDate,
            request.PageNumber,
            request.PageSize,
            now,
            ct);

        return result;

    }
}
