using TrainBooking.Application.Abstractions.Commands;

namespace TrainBooking.Application.Reservations.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid ReservationId) : ICommand;
