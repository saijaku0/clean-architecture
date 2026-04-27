using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.TripSeats.DomainEvents;
using TrainBooking.Domain.TripSeats.Enums;
using TrainBooking.Domain.TripSeats.Errors;

namespace TrainBooking.Domain.TripSeats;

public class TripSeat : AggregateRoot
{
    public Guid TripId { get; private init; }
    public Guid SeatId { get; private init; }
    public TripSeatStatus Status { get; private set; }
    public decimal Price { get; private init; }

    private TripSeat() { }
    private TripSeat(
        Guid id,
        Guid tripId,
        Guid seatId,
        decimal price) : base(id)
    {
        TripId = tripId;
        SeatId = seatId;
        Price = price;

        Status = TripSeatStatus.Available;
    }

    public static TripSeat Create(
        Guid tripId,
        Guid seatId,
        decimal price)
    {
        Guard.Against.Empty(tripId);
        Guard.Against.Empty(seatId);
        Guard.Against.NegativeOrZero(price);

        return new TripSeat(
            Guid.CreateVersion7(),
            tripId,
            seatId,
            price);
    }

    public Result Reserve()
    {
        Result checkResult = CheckAlreadySold();
        if (checkResult.IsFailure)
            return checkResult;

        if (Status != TripSeatStatus.Available)
            return TripSeatErrors.AlreadyReserved(Id);

        Status = TripSeatStatus.Reserved;
        AddDomainEvent(new TripSeatReservedDomainEvent(Id));
        return Result.Success();
    }

    public Result Release()
    {
        Result checkResult = CheckAlreadySold();
        if (checkResult.IsFailure)
            return checkResult;

        if (Status != TripSeatStatus.Reserved)
            return TripSeatErrors.NotReserved(Id);

        Status = TripSeatStatus.Available;
        AddDomainEvent(new TripSeatReleasedDomainEvent(Id));
        return Result.Success();
    }

    public Result MarkAsSold()
    {
        Result checkResult = CheckAlreadySold();
        if (checkResult.IsFailure)
            return checkResult;

        if (Status != TripSeatStatus.Reserved)
            return TripSeatErrors.NotReserved(Id);

        Status = TripSeatStatus.Sold;
        return Result.Success();
    }

    private Result CheckAlreadySold()
    {
        if (Status == TripSeatStatus.Sold)
            return TripSeatErrors.AlreadySold(Id);
        return Result.Success();
    }
}
