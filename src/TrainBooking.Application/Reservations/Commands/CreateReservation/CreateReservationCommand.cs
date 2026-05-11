using TrainBooking.Application.Abstractions.Commands;

namespace TrainBooking.Application.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(
    Guid TripId,
    IReadOnlyCollection<Guid> TripSeatIds) : ICommand<CreateReservationResult>;

public sealed record CreateReservationResult(
    Guid ReservationId,
    decimal TotalPrice,
    DateTime ExpiresAt);
