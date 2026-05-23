using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Queries;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Reservations.Queries.GetMyReservations;

internal sealed class GetMyReservationsQueryHandler(
    IReservationRepository reservationRepository,
    ICurrentUserService userService)
    : IQueryHandler<GetMyReservationsQuery, IReadOnlyCollection<ReservationListDto>>
{
    public async Task<Result<IReadOnlyCollection<ReservationListDto>>> Handle(
        GetMyReservationsQuery request,
        CancellationToken ct)
    {
        Guid? userId = userService.UserId;
        if (userId is null)
            return Error.Unauthorized("Reservations.Unauthorized", "Must be logged in.");

        IReadOnlyCollection<ReservationListDto> reservations =
            await reservationRepository.GetUserReservationsAsync(userId.Value, ct);

        return Result<IReadOnlyCollection<ReservationListDto>>.Success(reservations);
    }
}
