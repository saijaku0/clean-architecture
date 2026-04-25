using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.Trains.Errors;

public static class TrainErrors
{
    public static Error DuplicateWagonNumber(int number) =>
            Error.Conflict("train.duplicate_wagon_number", $"Wagon number {number} already exists in this train.");
}
