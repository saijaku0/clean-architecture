using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Trips.DomainEvent;
using TrainBooking.Domain.Trips.Errors;

namespace TrainBooking.Domain.Trips;

public class Trip : AggregateRoot
{
    public Guid TrainId { get; private init; }
    public string OriginStation { get; private init; } = string.Empty;
    public string DestinationStation { get; private init; } = string.Empty;
    public DateTime DepartureTime { get; private init; }
    public DateTime ArrivalTime { get; private init; }

    private Trip() { }
    private Trip(
        Guid id,
        Guid trainId,
        string originStation,
        string destinationStation,
        DateTime departureTime,
        DateTime arrivalTime) : base(id)
    {
        TrainId = trainId;
        OriginStation = originStation;
        DestinationStation = destinationStation;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        AddDomainEvent(new TripCreatedDomainEvent(id));
    }

    public static Result<Trip> Create(
        Guid trainId,
        string originStation,
        string destinationStation,
        DateTime departureTime,
        DateTime arrivalTime,
        TimeProvider timeProvider)
    {
        Guard.Against.Empty(trainId);
        Guard.Against.NullOrWhiteSpace(originStation);
        Guard.Against.NullOrWhiteSpace(destinationStation);

        if (departureTime >= arrivalTime)
            return TripErrors.InvalidTimeRange(departureTime, arrivalTime);

        if (originStation == destinationStation)
            return TripErrors.SameOriginAndDestination();

        if (departureTime < timeProvider.GetUtcNow().UtcDateTime)
            return TripErrors.DepartureTimeInPast(departureTime);

        return new Trip(
            Guid.CreateVersion7(),
            trainId,
            originStation,
            destinationStation,
            departureTime,
            arrivalTime);
    }
}
