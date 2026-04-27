using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.TripSeats.Errors;

public static class TripSeatErrors
{
    public static Error AlreadyReserved(Guid tripSeatId) =>
        Error.Conflict("trip_seat.already_reserved", $"Seat number {tripSeatId} is already reserved.");
    public static Error AlreadySold(Guid tripSeatId) =>
        Error.Conflict("trip_seat.already_sold", $"Seat number {tripSeatId} is already sold.");
    public static Error NotReserved(Guid tripSeatId) =>
        Error.Conflict("trip_seat.not_reserved", $"Seat number {tripSeatId} is not reserved.");
}
