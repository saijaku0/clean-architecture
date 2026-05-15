using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.Reservations.Errors;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Domain.Tests.Reservations;

public class ReservationConfirmTests
{
    private static readonly DateTime _fixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Confirm_WhenPendingAndNotExpired_ReturnsSuccessAndSetsConfirmedStatus()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        Reservation reservation = CreatePendingReservation(timeProvider);

        Result result = reservation.Confirm(timeProvider);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.ConfirmedAt.Should().Be(_fixedNow);
    }
    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ReturnsAlreadyConfirmedError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        Reservation reservation = CreatePendingReservation(timeProvider);

        Result firstResult = reservation.Confirm(timeProvider);
        Result secondResult = reservation.Confirm(timeProvider);

        secondResult.IsSuccess.Should().BeFalse();
        secondResult.Error.Should().Be(ReservationErrors.AlreadyConfirmed());
    }
    [Fact]
    public void Confirm_WhenAlreadyCancelled_ReturnsAlreadyCancelledError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        Reservation reservation = CreatePendingReservation(timeProvider);

        Result cancelResult = reservation.Cancel(timeProvider);
        cancelResult.IsSuccess.Should().BeTrue();

        Result result = reservation.Confirm(timeProvider);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservationErrors.AlreadyCancelled());
    }
    [Fact]
    public void Confirm_WhenExpired_ReturnsCannotConfirmExpiredError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        Reservation reservation = CreatePendingReservation(timeProvider);

        timeProvider.Advance(TimeSpan.FromMinutes(16));

        Result result = reservation.Confirm(timeProvider);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservationErrors.CannotConfirmExpired(reservation.ExpiresAt));
    }

    [Fact]
    public void Confirm_WhenAlreadyExpired_ReturnsAlreadyExpiredError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        Reservation reservation = CreatePendingReservation(timeProvider);

        timeProvider.Advance(TimeSpan.FromMinutes(16));

        Result expireResult = reservation.Expire(timeProvider);
        expireResult.IsSuccess.Should().BeTrue();

        Result result = reservation.Confirm(timeProvider);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservationErrors.AlreadyExpired());
    }

    // Helpers
    private static Reservation CreatePendingReservation(FakeTimeProvider timeProvider)
    {
        var userId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        DateTime departure = _fixedNow.AddDays(7);
        List<TripSeat> tripSeats = CreateValidTripSeats(tripId);

        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            departure,
            tripSeats,
            timeProvider);

        return result.Value;

    }
    private static List<TripSeat> CreateValidTripSeats(Guid tripId)
    {
        return
        [
            TripSeat.Create(tripId, Guid.NewGuid(), 100m),
            TripSeat.Create(tripId, Guid.NewGuid(), 150m)
        ];
    }
    private static FakeTimeProvider SetupFixedTimeProvider()
    {
        var fakeTimeProvider = new FakeTimeProvider(_fixedNow);
        return fakeTimeProvider;
    }
}
