using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Trips.GetTripById;

internal sealed class GetTripByIdQueryHandler(
    ITripRepository tripRepository) : IQueryHandler<GetTripByIdQuery, TripDetailsDto>
{
    public async Task<Result<TripDetailsDto>> Handle(
        GetTripByIdQuery request,
        CancellationToken ct)
    {
        TripDetailsDto? tripDto = await tripRepository.GetTripDetailsAsync(request.TripId, ct);

        if (tripDto is null)
        {
            return Error.NotFound(
                "Trips.NotFound",
                $"Trip with ID {request.TripId} not found.");
        }

        return tripDto;
    }
}
