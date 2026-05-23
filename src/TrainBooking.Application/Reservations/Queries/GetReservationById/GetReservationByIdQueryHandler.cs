using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Reservations.Queries.GetReservationById;

internal sealed class GetReservationByIdQueryHandler(
    IReservationRepository reservationRepository,
    ICurrentUserService userService)
    : IQueryHandler<GetReservationByIdQuery, ReservationDetailsDto>
{
    public async Task<Result<ReservationDetailsDto>> Handle(
        GetReservationByIdQuery request,
        CancellationToken ct)
    {
        Guid? userId = userService.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("Reservations.Unauthorized", "You must be authenticated to view reservation details.");
        }

        ReservationDetailsDto? reservation =
            await reservationRepository.GetReservationDetailsAsync(request.ReservationId, userId.Value, ct);

        if (reservation is null)
        {
            return Error.NotFound(
                "Reservations.NotFound",
                $"Reservation '{request.ReservationId}' not found.");
        }

        return reservation;
    }
}
