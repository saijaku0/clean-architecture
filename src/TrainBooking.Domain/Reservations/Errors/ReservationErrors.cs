using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.Reservations.Errors;

public static class ReservationErrors
{
    public static Error NoSeatSelected() =>
        Error.Validation("reservation.no_seat_selected", "No seat selected. At least one seat must be selected.");
    public static Error TooManySeatsSelected(int seatCount) =>
        Error.Validation("reservation.too_many_seats_selected", $"Too many seats selected. Seat count: {seatCount}. Maximum allowed is 4.");
    public static Error TripAlreadyDeparted(DateTime tripDepartureTime) =>
        Error.Validation("reservation.trip_already_departed", $"Trip has already departed. TripDepartureTime: {tripDepartureTime}.");
    public static Error CannotConfirmExpired(DateTime expiresAt) =>
        Error.Conflict("reservation.cannot_confirm_expired", $"Cannot confirm reservation. Reservation expired at: {expiresAt}.");
    public static Error CannotCancelWithinDepartureWindow(DateTime tripDepartureTime, int requiredHours) =>
        Error.Conflict("reservation.cannot_cancel_within_departure_window", $"Cannot cancel reservation within {requiredHours} hours of trip departure. TripDepartureTime: {tripDepartureTime}.");
    public static Error CannotExpireNotYetExpired(DateTime now, DateTime expiresAt) =>
        Error.Conflict("reservation.cannot_expire_not_yet_expired", $"Cannot expire reservation. Current time: {now}. Reservation expires at: {expiresAt}.");
    public static Error AlreadyConfirmed() =>
        Error.Conflict("reservation.already_confirmed", "Reservation has already been confirmed.");
    public static Error AlreadyCancelled() =>
        Error.Conflict("reservation.already_cancelled", "Reservation has already been cancelled.");
    public static Error AlreadyExpired() =>
        Error.Conflict("reservation.already_expired", "Reservation has already expired.");
    public static Error ReservationNotPending() =>
        Error.Validation("reservation.not_pending", "Reservation is not in pending status.");
}
