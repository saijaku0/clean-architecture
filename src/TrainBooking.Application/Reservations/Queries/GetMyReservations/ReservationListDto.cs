namespace TrainBooking.Application.Reservations.Queries.GetMyReservations;

public sealed record ReservationListDto(
    Guid Id,
    string Status,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    Guid TripId,
    string TrainName,
    string OriginStation,
    string DestinationStation,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    IReadOnlyCollection<string> SeatLabels,
    decimal TotalPrice);
