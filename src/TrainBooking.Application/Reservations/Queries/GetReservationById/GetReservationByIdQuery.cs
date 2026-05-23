using TrainBooking.Application.Abstractions.Queries;

namespace TrainBooking.Application.Reservations.Queries.GetReservationById;

public sealed record GetReservationByIdQuery(Guid ReservationId) : IQuery<ReservationDetailsDto>;
