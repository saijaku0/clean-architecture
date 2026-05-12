using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Trains;
using TrainBooking.Domain.Trains.ValueObjects;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Seed;

internal sealed class DataFactory(TimeProvider timeProvider)
{
    public Train CreateTrain(string name, int wagonCount, int seatsPerWagon, SeatClass wagonClass)
    {
        var train = Train.Create(name);

        for (int wagonNumber = 1; wagonNumber <= wagonCount; wagonNumber++)
        {
            Result addWagon = train.AddWagon(wagonNumber, wagonClass);
            ThrowIfFailure(addWagon, $"add wagon {wagonNumber}");

            for (int seatNumber = 1; seatNumber <= seatsPerWagon; seatNumber++)
            {
                Result addSeat = train.AddSeatToWagon(wagonNumber, seatNumber);
                ThrowIfFailure(addSeat, $"add seat {seatNumber} to wagon {wagonNumber}");
            }
        }

        return train;
    }

    public Trip CreateTrip(
        Guid trainId,
        string origin,
        string destination,
        DateTime departure,
        TimeSpan duration)
    {
        Result<Trip> result = Trip.Create(
            trainId,
            origin,
            destination,
            departure,
            departure + duration,
            timeProvider);

        ThrowIfFailure(result, "create trip");
        return result.Value;
    }

    public List<TripSeat> CreateTripSeatsForTrain(Guid tripId, Train train, decimal basePrice)
    {
        var tripSeats = new List<TripSeat>();

        foreach (Wagon wagon in train.Wagons)
        {
            decimal priceForWagon = wagon.Class.CalculatePrice(basePrice);

            foreach (Seat seat in wagon.Seats)
            {
                var tripSeat = TripSeat.Create(tripId, seat.Id, priceForWagon);
                tripSeats.Add(tripSeat);
            }
        }

        return tripSeats;
    }

    private static void ThrowIfFailure(Result result, string operation)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(
                $"Seed failed to {operation}: [{result.Error?.Code}] {result.Error?.Message}");
    }

    private static void ThrowIfFailure<T>(Result<T> result, string operation)
    {
        if (result.IsFailure)
            throw new InvalidOperationException(
                $"Seed failed to {operation}: [{result.Error?.Code}] {result.Error?.Message}");
    }
}
