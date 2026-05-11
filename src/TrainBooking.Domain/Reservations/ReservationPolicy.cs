namespace TrainBooking.Domain.Reservations;

public static class ReservationPolicy
{
    public const int MinSeats = 1;
    public const int MaxSeats = 4;
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan MinTimeBeforeDepartureForCancellation = TimeSpan.FromHours(24);
}
