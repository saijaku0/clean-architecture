using FluentValidation;
using FluentValidation.Results;
using TrainBooking.Application.Abstractions.Commands;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.TripSeats.Enums;

namespace TrainBooking.Application.Reservations.Commands.CreateReservation;

internal sealed class CreateReservationCommandHandler(
    ICurrentUserService currentUser,
    ITripRepository tripRepository,
    ITripSeatRepository tripSeatRepository,
    IReservationRepository reservationRepository,
    IValidator<CreateReservationCommand> validator,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : CommandHandler<CreateReservationCommand, CreateReservationResult>(unitOfWork)
{
    private readonly ICurrentUserService _currentUser = currentUser;
    private readonly ITripRepository _tripRepository = tripRepository;
    private readonly ITripSeatRepository _tripSeatRepository = tripSeatRepository;
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IValidator<CreateReservationCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task<Result<CreateReservationResult>> HandleAsync(
        CreateReservationCommand request,
        CancellationToken ct)
    {
        ValidationResult validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            string message = string.Join(
                "; ",
                validation.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            return Error.Validation("Reservations.Validation", message);
        }

        Guid? userId = _currentUser.UserId;
        if (userId is null)
            return Error.Unauthorized(
                "Reservations.NoAuthenticatedUser",
                "Authenticated user required to create a reservation.");

        await using IDbTransaction tx = await _unitOfWork.BeginTransactionAsync(ct);

        Trip? trip = await _tripRepository.GetByIdAsync(request.TripId, ct);
        if (trip is null)
            return Error.NotFound(
                "Reservations.TripNotFound",
                $"Trip '{request.TripId}' was not found.");

        IReadOnlyCollection<TripSeat> lockedSeats =
            await _tripSeatRepository.LockByIdsAsync(request.TripSeatIds, ct);

        if (lockedSeats.Count != request.TripSeatIds.Count)
        {
            HashSet<Guid> foundIds = [.. lockedSeats.Select(s => s.Id)];
            IEnumerable<Guid> missingIds = request.TripSeatIds.Where(id => !foundIds.Contains(id));
            return Error.NotFound(
                "Reservations.SeatsNotFound",
                $"Trip seats not found: {string.Join(", ", missingIds)}.");
        }

        IReadOnlyCollection<Guid> unavailableIds =
            [.. lockedSeats
                .Where(s => s.Status != TripSeatStatus.Available)
                .Select(s => s.Id)];

        if (unavailableIds.Count > 0)
            return Error.Conflict(
                "Reservations.SeatsNotAvailable",
                $"Trip seats already reserved or sold: {string.Join(", ", unavailableIds)}.");

        Result<Reservation> reservationResult = Reservation.Create(
            request.TripId,
            userId.Value,
            trip.DepartureTime,
            lockedSeats,
            _timeProvider);

        if (reservationResult.IsFailure)
            return reservationResult.Error!;

        Reservation reservation = reservationResult.Value;

        foreach (TripSeat seat in lockedSeats)
            seat.Reserve();

        await _reservationRepository.AddAsync(reservation, ct);

        await _unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return new CreateReservationResult(
            reservation.Id,
            reservation.TotalPrice,
            reservation.ExpiresAt);
    }
}
