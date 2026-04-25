using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Trains.DomainEvents;
using TrainBooking.Domain.Trains.Errors;
using TrainBooking.Domain.Trains.ValueObjects;

namespace TrainBooking.Domain.Trains;

public class Train : AggregateRoot
{
    private readonly List<Wagon> _wagons = [];
    public IReadOnlyCollection<Wagon> Wagons => _wagons.AsReadOnly();

    public string Name { get; private set; } = string.Empty;

    private Train() { }

    private Train(Guid id, string name) : base(id)
    {
        Name = name;
        AddDomainEvent(new TrainCreatedDomainEvent(id, name));
    }

    public static Train Create(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, 200);

        return new Train(Guid.CreateVersion7(), name);
    }

    public Result AddWagon(int wagonNumber, SeatClass wagonClass)
    {
        Guard.Against.NegativeOrZero(wagonNumber);
        Guard.Against.Null(wagonClass);
        if (_wagons.Any(w => w.Number == wagonNumber))
            return TrainErrors.DuplicateWagonNumber(wagonNumber);

        var newWagon = Wagon.Create(Id, wagonNumber, wagonClass);
        _wagons.Add(newWagon);
        return Result.Success();
    }

    public Result AddSeatToWagon(int wagonNumber, int seatNumber)
    {
        Guard.Against.NegativeOrZero(wagonNumber);
        Guard.Against.NegativeOrZero(seatNumber);
        Wagon? wagon = _wagons.FirstOrDefault(w => w.Number == wagonNumber);
        if (wagon is null)
            return TrainErrors.WagonNotFound(wagonNumber);
        return wagon.AddSeat(seatNumber);
    }
}
