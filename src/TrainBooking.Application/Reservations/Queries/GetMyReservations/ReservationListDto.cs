namespace TrainBooking.Application.Reservations.Queries.GetMyReservations;

public sealed record ReservationListDto(
    Guid Id,
    Guid TripId,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt);

