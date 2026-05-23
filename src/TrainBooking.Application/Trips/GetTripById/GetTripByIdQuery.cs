using TrainBooking.Application.Abstractions.Queries;

namespace TrainBooking.Application.Trips.GetTripById;

public sealed record GetTripByIdQuery(
    Guid TripId) : IQuery<TripDetailsDto>;
