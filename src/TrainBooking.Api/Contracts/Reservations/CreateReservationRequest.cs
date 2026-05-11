namespace TrainBooking.Api.Contracts.Reservations;

public sealed record CreateReservationRequest(
    Guid TripId,
    IReadOnlyCollection<Guid> TripSeatIds);
