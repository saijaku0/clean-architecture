using TrainBooking.Application.Abstractions.Queries;

namespace TrainBooking.Application.Reservations.Queries.GetMyReservations;

public sealed record GetMyReservationsQuery() : IQuery<IReadOnlyCollection<ReservationListDto>>;
