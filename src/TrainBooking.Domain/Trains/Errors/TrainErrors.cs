using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.Trains.Errors;

public static class TrainErrors
{
    public static Error DuplicateWagonNumber(int number) =>
            Error.Conflict("train.duplicate_wagon_number", $"Wagon number {number} already exists in this train.");
    public static Error DuplicateSeatNumber(int number) =>
            Error.Conflict("train.duplicate_seat_number", $"Seat number {number} already exists in this train.");
    public static Error WagonNotFound(int wagonNumber) =>
            Error.NotFound("train.wagon_not_found", $"Wagon with number {wagonNumber} not found in this train.");
}
