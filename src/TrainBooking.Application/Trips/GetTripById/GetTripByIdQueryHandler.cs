using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Trips.GetTripById;

internal sealed class GetTripByIdQueryHandler(
    ITripRepository tripRepository) : IQueryHandler<GetTripByIdQuery, GetTripByIdDto>
{
    public async Task<Result<GetTripByIdDto>> Handle(
        GetTripByIdQuery request,
        CancellationToken ct)
    {
        GetTripByIdDto? tripDto = await tripRepository.GetTripDetailsAsync(request.TripId, ct);

        if (tripDto is null)
        {
            return Error.NotFound(
                "Trips.NotFound",
                $"Trip with ID {request.TripId} not found.");
        }

        return tripDto;
    }
}
