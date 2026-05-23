namespace TrainBooking.Application.Reservations.Queries.GetReservationById;

public sealed record ReservationDetailsDto(
    Guid Id,
    Guid TripId,
    string OriginStation,
    string DestinationStation,
    DateTime DepartureTime,
    decimal TotalPrice,
    string Status,
    IReadOnlyCollection<Guid> SeatIds);
