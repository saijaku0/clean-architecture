using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.Trips.Errors;

public static class TripErrors
{
    public static Error InvalidTimeRange(DateTime departureTime, DateTime arrivalTime) =>
        Error.Validation("trip.invalid_time_range", $"Departure time ({departureTime}) must be before arrival time ({arrivalTime}).");

    public static Error SameOriginAndDestination() =>
        Error.Validation("trip.same_origin_destination", "Origin and destination stations cannot be the same.");

    public static Error DepartureTimeInPast(DateTime departureTime) =>
        Error.Validation("trip.departure_time_in_past", $"Departure time ({departureTime}) cannot be in the past.");
}
