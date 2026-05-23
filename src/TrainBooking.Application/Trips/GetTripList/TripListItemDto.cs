namespace TrainBooking.Application.Trips.GetTripList;

public sealed record TripListItemDto(
    Guid TripId,
    string OriginStation,
    string DestinationStation,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    int AvailableSeatsCount,
    decimal MinPrice);
