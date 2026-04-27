using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;

namespace TrainBooking.Domain.Reservations;

public class ReservationSeat : EntityBase
{
    public Guid ReservationId { get; private init; }
    public Guid TripSeatId { get; private init; }
    public decimal PriceSnapshot { get; private init; }

    private ReservationSeat() { }
    private ReservationSeat(
        Guid id,
        Guid reservationId,
        Guid tripSeatId,
        decimal priceSnapshot) : base(id)
    {
        ReservationId = reservationId;
        TripSeatId = tripSeatId;
        PriceSnapshot = priceSnapshot;
    }

    internal static ReservationSeat Create(
        Guid reservationId,
        Guid tripSeatId,
        decimal priceSnapshot)
    {
        Guard.Against.Empty(reservationId);
        Guard.Against.Empty(tripSeatId);
        Guard.Against.NegativeOrZero(priceSnapshot);

        return new ReservationSeat(
            Guid.CreateVersion7(),
            reservationId,
            tripSeatId,
            priceSnapshot);
    }
}
