namespace TrainBooking.Application.Trips.GetTripById;

public sealed record TripDetailsDto(
    Guid TripId,
    string OriginStation,
    string DestinationStation,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    string TrainName,
    IReadOnlyList<WagonDto> Wagons);

public sealed record WagonDto(
    int WagonNumber,
    string WagonClass,
    IReadOnlyList<SeatDto> Seats);

public sealed record SeatDto(
    Guid TripSeatId,
    int SeatNumber,
    string Status,
    decimal Price);
