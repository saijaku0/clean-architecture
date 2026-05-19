using TrainBooking.Application.Abstractions.Commands;

namespace TrainBooking.Application.Reservations.Commands.ConfirmReservation;

public sealed record ConfirmReservationCommand(
    Guid ReservationId) : ICommand<ConfirmReservationResult>;

public sealed record ConfirmReservationResult(
    Guid ReservationId,
    Guid TripId,
    IReadOnlyCollection<Guid> TripSeatIds,
    decimal TotalPrice,
    DateTime ConfirmedAt);
