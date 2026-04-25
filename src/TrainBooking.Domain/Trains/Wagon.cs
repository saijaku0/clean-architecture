using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Trains.Errors;
using TrainBooking.Domain.Trains.ValueObjects;

namespace TrainBooking.Domain.Trains;

public class Wagon : EntityBase
{
    private readonly List<Seat> _seats = [];
    public IReadOnlyCollection<Seat> Seats => _seats.AsReadOnly();

    public Guid TrainId { get; private init; }

    public int Number { get; private init; }
    public SeatClass Class { get; private init; } = null!;

    private Wagon() { }

    private Wagon(
        Guid id,
        Guid trainId,
        int wagonNumber,
        SeatClass wagonClass) : base(id)
    {
        TrainId = trainId;
        Number = wagonNumber;
        Class = wagonClass;
    }

    internal static Wagon Create(
        Guid trainId,
        int wagonNumber,
        SeatClass wagonClass)
    {
        Guard.Against.Empty(trainId);
        Guard.Against.Null(wagonClass);
        Guard.Against.NegativeOrZero(wagonNumber);

        return new Wagon(
            Guid.CreateVersion7(),
            trainId,
            wagonNumber,
            wagonClass);
    }

    internal Result AddSeat(int seatNumber)
    {
        Guard.Against.NegativeOrZero(seatNumber);
        if (_seats.Any(s => s.Number == seatNumber))
            return TrainErrors.DuplicateSeatNumber(seatNumber);
        var newSeat = Seat.Create(Id, seatNumber);
        _seats.Add(newSeat);
        return Result.Success();
    }

    /// <remarks>
    /// In the future, methods for updating and modifying the entity may be added here.
    /// </remarks>
}
