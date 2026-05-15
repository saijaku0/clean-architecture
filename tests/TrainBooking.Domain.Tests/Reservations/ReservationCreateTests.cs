using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.Reservations.Errors;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Domain.Tests.Reservations;

public class ReservationCreateTests
{
    private static readonly DateTime _fixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _departure = _fixedNow.AddDays(1);
    private static List<TripSeat> CreateValidTripSeats(Guid tripId)
    {
        return
        [
            TripSeat.Create(tripId, Guid.NewGuid(), 100m),
            TripSeat.Create(tripId, Guid.NewGuid(), 100m)
        ];
    }

    private static FakeTimeProvider SetupFixedTimeProvider()
    {
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetUtcNow(_fixedNow);
        return timeProvider;
    }

    [Fact]
    public void Create_WithValidInputs_ReturnsSuccessfulReservation()
    {

        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        List<TripSeat> tripSeats = CreateValidTripSeats(tripId);

        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            _departure,
            tripSeats,
            timeProvider);

        result.IsSuccess.Should().BeTrue();

        Reservation reservation = result.Value;

        reservation.TripId.Should().Be(tripId);
        reservation.UserId.Should().Be(userId);
        reservation.TripDepartureTime.Should().Be(_departure);
        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.TotalPrice.Should().Be(200m);
        reservation.ExpiresAt.Should().Be(_fixedNow.AddMinutes(15));
        reservation.ReservationSeats.Should().HaveCount(2);
        reservation.ConfirmedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyTripId_Throws()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var userId = Guid.NewGuid();
        Guid emptyTripId = Guid.Empty;
        List<TripSeat> tripSeats = CreateValidTripSeats(Guid.NewGuid());
        Action act = () => Reservation.Create(
            emptyTripId,
            userId,
            _departure,
            tripSeats,
            timeProvider);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyUserId_Throws()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        Guid emptyUserId = Guid.Empty;
        List<TripSeat> tripSeats = CreateValidTripSeats(tripId);
        Action act = () => Reservation.Create(
            tripId,
            emptyUserId,
            _departure,
            tripSeats,
            timeProvider);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullTripSeats_Throws()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        List<TripSeat> nullTripSeats = null!;
        Action act = () => Reservation.Create(
            tripId,
            userId,
            _departure,
            nullTripSeats,
            timeProvider);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithZeroSeats_ReturnsNoSeatSelectedError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        List<TripSeat> emptyTripSeats = [];
        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            _departure,
            emptyTripSeats,
            timeProvider);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expected: ReservationErrors.NoSeatSelected());
    }

    [Fact]
    public void Create_WithFiveSeats_ReturnsTooManySeatsError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        List<TripSeat> tooManySeats = CreateValidTripSeats(tripId);
        tooManySeats.Add(TripSeat.Create(tripId, Guid.NewGuid(), 100m));
        tooManySeats.Add(TripSeat.Create(tripId, Guid.NewGuid(), 100m));
        tooManySeats.Add(TripSeat.Create(tripId, Guid.NewGuid(), 100m));
        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            _departure,
            tooManySeats,
            timeProvider);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expected: ReservationErrors.TooManySeatsSelected(
            tooManySeats.Count, ReservationPolicy.MaxSeats));
    }

    [Fact]
    public void Create_WithSeatsFromDifferentTrips_ReturnsSeatsFromDifferentTripsError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        List<TripSeat> tripSeats = CreateValidTripSeats(tripId);
        tripSeats.Add(TripSeat.Create(Guid.NewGuid(), Guid.NewGuid(), 100m));
        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            _departure,
            tripSeats,
            timeProvider);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(expected: ReservationErrors.SeatsFromDifferentTrips());
    }

    [Fact]
    public void Create_WithPastDepartureTime_ReturnsTripAlreadyDepartedError()
    {
        FakeTimeProvider timeProvider = SetupFixedTimeProvider();
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        DateTime pastDeparture = _fixedNow.AddHours(-1);
        List<TripSeat> tripSeats = CreateValidTripSeats(tripId);
        Result<Reservation> result = Reservation.Create(
            tripId,
            userId,
            pastDeparture,
            tripSeats,
            timeProvider);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservationErrors.TripAlreadyDeparted(pastDeparture));
    }
}
