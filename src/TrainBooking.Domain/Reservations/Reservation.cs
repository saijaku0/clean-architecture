using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations.DomainEvents;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.Reservations.Errors;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Domain.Reservations;

public class Reservation : AggregateRoot
{
    public Guid TripId { get; private init; }
    public Guid UserId { get; private init; }
    public decimal TotalPrice { get; private init; }
    public ReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private init; }
    public DateTime TripDepartureTime { get; private init; }
    public DateTime? ConfirmedAt { get; private set; }

    private readonly List<ReservationSeat> _reservationSeats = [];
    public IReadOnlyCollection<ReservationSeat> ReservationSeats => _reservationSeats.AsReadOnly();

    private Reservation() { }
    private Reservation(
        Guid id,
        Guid tripId,
        Guid userId,
        decimal totalPrice,
        DateTime expiresAt,
        DateTime tripDepartureTime,
        IEnumerable<ReservationSeat> reservationSeats) : base(id)
    {
        TripId = tripId;
        UserId = userId;
        TotalPrice = totalPrice;
        TripDepartureTime = tripDepartureTime;
        ExpiresAt = expiresAt;
        Status = ReservationStatus.Pending;

        _reservationSeats.AddRange(reservationSeats);

        AddDomainEvent(new ReservationCreatedDomainEvent(Id));
    }

    public static Result<Reservation> Create(
        Guid tripId,
        Guid userId,
        DateTime tripDepartureTime,
        IReadOnlyCollection<TripSeat> tripSeats,
        TimeProvider timeProvider)
    {
        Guard.Against.Empty(tripId);
        Guard.Against.Empty(userId);
        Guard.Against.Null(tripSeats);

        if (tripSeats.Count == 0)
            return ReservationErrors.NoSeatSelected();

        if (tripSeats.Count > 4)
            return ReservationErrors.TooManySeatsSelected(tripSeats.Count);

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (tripDepartureTime <= now)
            return ReservationErrors.TripAlreadyDeparted(tripDepartureTime);

        var reservationSeats = new List<ReservationSeat>();
        DateTime expiresAt = now.AddMinutes(15);
        decimal totalPrice = tripSeats.Sum(ts => ts.Price);
        var reservationId = Guid.CreateVersion7();

        foreach (TripSeat tripSeat in tripSeats)
        {
            var reservationSeat = ReservationSeat.Create(
                reservationId,
                tripSeat.Id,
                tripSeat.Price);
            reservationSeats.Add(reservationSeat);
        }

        return new Reservation(
            reservationId,
            tripId,
            userId,
            totalPrice,
            expiresAt,
            tripDepartureTime,
            reservationSeats);
    }

    public Result Confirm(TimeProvider timeProvider)
    {
        Result pendingResult = EnsurePending();
        if (pendingResult.IsFailure)
            return pendingResult;

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (now > ExpiresAt)
            return ReservationErrors.CannotConfirmExpired(ExpiresAt);

        Status = ReservationStatus.Confirmed;
        ConfirmedAt = now;
        AddDomainEvent(new ReservationConfirmedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(TimeProvider timeProvider)
    {
        Result pendingResult = EnsurePending();
        if (pendingResult.IsFailure)
            return pendingResult;

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        double hoursUntilDeparture = (TripDepartureTime - now).TotalHours;
        const int REQUIRED_HOURS = 24;
        if (hoursUntilDeparture < REQUIRED_HOURS)
            return ReservationErrors.CannotCancelWithinDepartureWindow(TripDepartureTime, REQUIRED_HOURS);

        Status = ReservationStatus.Cancelled;
        AddDomainEvent(new ReservationCancelledDomainEvent(Id));

        return Result.Success();
    }

    public Result Expire(TimeProvider timeProvider)
    {
        Result pendingResult = EnsurePending();
        if (pendingResult.IsFailure)
            return pendingResult;

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (now < ExpiresAt)
            return ReservationErrors.CannotExpireNotYetExpired(now, ExpiresAt);

        Status = ReservationStatus.Expired;
        AddDomainEvent(new ReservationExpiredDomainEvent(Id));

        return Result.Success();
    }

    private Result EnsurePending()
    {
        return Status switch
        {
            ReservationStatus.Pending => Result.Success(),
            ReservationStatus.Confirmed => ReservationErrors.AlreadyConfirmed(),
            ReservationStatus.Cancelled => ReservationErrors.AlreadyCancelled(),
            ReservationStatus.Expired => ReservationErrors.AlreadyExpired(),
            _ => throw new InvalidOperationException($"Unknown status: {Status}")
        };
    }
}
