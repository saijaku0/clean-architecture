using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Trains.ValueObjects;

namespace TrainBooking.Domain.Trains;

public class Wagon : EntityBase
{
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

    /// <remarks>
    /// In the future, methods for updating and modifying the entity may be added here.
    /// </remarks>
}
