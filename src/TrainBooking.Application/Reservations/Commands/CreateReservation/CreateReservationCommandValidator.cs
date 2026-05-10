using FluentValidation;
using TrainBooking.Domain.Reservations;

namespace TrainBooking.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandValidator
    : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.TripId)
            .NotEmpty();
        RuleFor(x => x.TripSeatIds)
            .NotEmpty()
            .Must(BeWithinReservationLimit).WithMessage("Reservation must contain between 1 and 4 trip seats.")
            .Must(BeUnique).WithMessage("Trip seat IDs must be unique within a reservation.");

        RuleForEach(x => x.TripSeatIds)
            .NotEmpty().WithMessage("Trip seat ID cannot be empty.");
    }

    private static bool BeWithinReservationLimit(IReadOnlyCollection<Guid> ids) =>
        ids.Count is >= ReservationPolicy.MinSeats and <= ReservationPolicy.MaxSeats;

    private static bool BeUnique(IReadOnlyCollection<Guid> ids) =>
        ids.Distinct().Count() == ids.Count;
}
