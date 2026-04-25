namespace TrainBooking.Domain.Trains.ValueObjects;

/// <summary>
/// Value object representing a seat record in a train.
/// </summary>
public sealed record SeatClass(string Name, decimal PriceMultiplier)
{
    public static readonly SeatClass FirstClass = new("FirstClass", 1.5m);
    public static readonly SeatClass SecondClass = new("SecondClass", 1.0m);
    public static readonly SeatClass Bistro = new("Bistro", 0.0m);

    public decimal CalculatePrice(decimal basePrice) => basePrice * PriceMultiplier;

    public static SeatClass FromName(string name) =>
        _seatClasses.TryGetValue(name, out SeatClass? seatClass)
            ? seatClass
            : throw new ArgumentException($"Unknown seat class: {name}");

    private static readonly Dictionary<string, SeatClass> _seatClasses = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(FirstClass)] = FirstClass,
        [nameof(SecondClass)] = SecondClass,
        [nameof(Bistro)] = Bistro
    };
}
