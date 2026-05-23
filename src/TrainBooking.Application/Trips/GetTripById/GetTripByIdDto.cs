namespace TrainBooking.Application.Trips.GetTripById;

public sealed record GetTripByIdDto(
    Guid TripId,
    string OriginStation,
    string DestinationStation,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    int AvailableSeatsCount,
    decimal MinPrice);
