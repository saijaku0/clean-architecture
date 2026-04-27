using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;

namespace TrainBooking.Domain.Trains;

public class Seat : EntityBase
{
    public Guid WagonId { get; private init; }
    public int Number { get; private init; }

    private Seat() { }
    private Seat(
        Guid id,
        Guid wagonId,
        int number) : base(id)
    {
        WagonId = wagonId;
        Number = number;
    }

    internal static Seat Create(
        Guid wagonId,
        int number)
    {
        Guard.Against.Empty(wagonId);
        Guard.Against.NegativeOrZero(number);

        return new Seat(
            Guid.CreateVersion7(),
            wagonId,
            number);
    }
}
