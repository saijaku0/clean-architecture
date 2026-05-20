using FluentValidation;

namespace TrainBooking.Application.Reservations.Commands.ConfirmReservation;

public sealed class ConfirmReservationCommandValidator : AbstractValidator<ConfirmReservationCommand>
{
    public ConfirmReservationCommandValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotEmpty()
            .WithMessage("Reservation ID is required.");
        RuleFor(x => x.PaymentToken).NotEmpty();
        RuleFor(x => x.AvailableFunds).GreaterThan(0);
    }
}
