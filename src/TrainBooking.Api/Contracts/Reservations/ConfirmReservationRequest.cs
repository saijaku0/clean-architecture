namespace TrainBooking.Api.Contracts.Reservations;

public sealed record ConfirmReservationRequest(
    string PaymentToken,
    decimal AvailableFunds);
