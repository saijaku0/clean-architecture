using TrainBooking.Application.Abstractions.Commands;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Application.Reservations.Commands.CancelReservation;

internal sealed class CancelReservationCommandHandler(
    IReservationRepository reservationRepository,
    ITripSeatRepository tripSeatRepository,
    ICurrentUserService userService,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork) : CommandHandler<CancelReservationCommand>(unitOfWork)
{
    protected override async Task<Result> HandleAsync(
        CancelReservationCommand request,
        CancellationToken ct)
    {
        Guid? user = userService.UserId;
        if (user is null)
        {
            return Error.Unauthorized(
                "Reservations.CancelReservation.Unauthorized",
                "You must be authenticated to cancel a reservation.");
        }

        Reservation? reservation = await reservationRepository.GetByIdAsync(request.ReservationId, ct);
        if (reservation is null)
        {
            return Error.NotFound(
                "Reservations.CancelReservation.ReservationNotFound",
                $"Reservation '{request.ReservationId}' was not found.");
        }

        if (user != reservation.UserId)
        {
            return Error.Forbidden(
                "Reservations.CancelReservation.Forbidden",
                "You are not the owner of this reservation.");
        }

        Result cancelResult = reservation.Cancel(timeProvider);
        if (cancelResult.IsFailure)
        {
            return cancelResult.Error!;
        }

        var tripSeatIds = reservation.ReservationSeats
            .Select(rs => rs.TripSeatId)
            .ToList();

        IReadOnlyCollection<TripSeat> tripSeats = await tripSeatRepository.GetByIdsAsync(tripSeatIds, ct);

        foreach (TripSeat tripSeat in tripSeats)
        {
            Result markAsAvailableResult = tripSeat.Release();
            if (markAsAvailableResult.IsFailure)
            {
                return markAsAvailableResult.Error!;
            }
        }

        return Result.Success();
    }
}
