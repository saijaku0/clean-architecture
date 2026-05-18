using TrainBooking.Application.Abstractions.Commands;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Application.Reservations.Commands.ConfirmReservation;

internal sealed class ConfirmReservationCommandHandler(
    IReservationRepository reservationRepository,
    ICurrentUserService userService,
    ITripSeatRepository tripSeatRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : CommandHandler<ConfirmReservationCommand, ConfirmReservationResult>(unitOfWork)
{
    protected override async Task<Result<ConfirmReservationResult>> HandleAsync(
        ConfirmReservationCommand request,
        CancellationToken ct)
    {
        Guid? user = userService.UserId ??
            throw new UnauthorizedAccessException("User is not authenticated.");

        Reservation? reservation = await reservationRepository.GetByIdAsync(request.ReservationId, ct);
        if (reservation is null)
            return Error.NotFound(
                "Reservations.ReservationNotFound",
                $"Reservation '{request.ReservationId}' was not found.");

        if (user != reservation.UserId)
        {
            return Error.Forbidden(
                "Reservations.ConfirmReservation.Forbidden",
                "You are not the owner of this reservation.");
        }

        var tripSeatIds = reservation.ReservationSeats
            .Select(rs => rs.TripSeatId)
            .ToList();

        IReadOnlyCollection<TripSeat> tripSeats = await tripSeatRepository.GetByIdsAsync(tripSeatIds, ct);

        reservation.Confirm(timeProvider);
        foreach (TripSeat tripSeat in tripSeats)
            tripSeat.MarkAsSold();
        await _unitOfWork.CommitAsync(ct);

        return new ConfirmReservationResult(
            reservation.Id,
            reservation.TripId,
            [.. tripSeats.Select(ts => ts.Id)],
            reservation.TotalPrice
        );
    }
}
