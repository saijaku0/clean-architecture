using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TrainBooking.Application;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Reservations.Commands.CreateReservation;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.TripSeats.Enums;
using TrainBooking.Infrastructure;
using TrainBooking.Infrastructure.Persistence;
using TrainBooking.IntegrationTests.Infrastructure;

namespace TrainBooking.IntegrationTests.Reservations;

[Collection("IntegrationTests")]
public class CreateReservationCommandHandlerTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task Handle_WithValidRequest_CreatesReservationAndReservesSeats()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        IServiceProvider services = BuildServiceProvider(data.UserId);
        ISender mediator = services.GetRequiredService<ISender>();

        var command = new CreateReservationCommand(
            TripId: data.TripId,
            TripSeatIds: [.. data.TripSeatIds.Take(2)]);

        Result<CreateReservationResult> result = await mediator.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.ReservationId.Should().NotBe(Guid.Empty);
        result.Value.TotalPrice.Should().Be(200m);
        result.Value.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        await using AppDbContext verifyContext = fixture.CreateDbContext();

        Reservation? reservation = await verifyContext.Reservations
            .Include(r => r.ReservationSeats)
            .SingleOrDefaultAsync(r => r.Id == result.Value.ReservationId);

        reservation.Should().NotBeNull();
        reservation!.UserId.Should().Be(data.UserId);
        reservation.TripId.Should().Be(data.TripId);
        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.TotalPrice.Should().Be(200m);
        reservation.ReservationSeats.Should().HaveCount(2);

        List<TripSeat> lockedSeats = await verifyContext.TripSeats
            .Where(ts => data.TripSeatIds.Take(2).Contains(ts.Id))
            .ToListAsync();

        lockedSeats.Should().HaveCount(2);
        lockedSeats.Should().AllSatisfy(s => s.Status.Should().Be(TripSeatStatus.Reserved));

        List<TripSeat> stillAvailable = await verifyContext.TripSeats
            .Where(ts => data.TripSeatIds.Skip(2).Contains(ts.Id))
            .ToListAsync();

        stillAvailable.Should().AllSatisfy(s => s.Status.Should().Be(TripSeatStatus.Available));
    }

    [Fact]
    public async Task Handle_WhenSeatAlreadyReserved_ReturnsSeatsNotAvailableError()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        IServiceProvider services = BuildServiceProvider(data.UserId);
        ISender mediator = services.GetRequiredService<ISender>();

        var firstCommand = new CreateReservationCommand(
            TripId: data.TripId,
            TripSeatIds: [data.TripSeatIds[0]]);

        Result<CreateReservationResult> firstResult = await mediator.Send(firstCommand);
        firstResult.IsSuccess.Should().BeTrue();

        var secondCommand = new CreateReservationCommand(
            TripId: data.TripId,
            TripSeatIds: [data.TripSeatIds[0]]);

        Result<CreateReservationResult> secondResult = await mediator.Send(secondCommand);

        secondResult.IsFailure.Should().BeTrue();
        secondResult.Error!.Code.Should().Be("Reservations.SeatsNotAvailable");
        secondResult.Error.Type.Should().Be(ErrorType.Conflict);

        await using AppDbContext verifyContext = fixture.CreateDbContext();
        int count = await verifyContext.Reservations.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_With10ParallelRequestsForSameSeat_ExactlyOneSucceeds()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        Guid contestedSeat = data.TripSeatIds[0];

        const int parallelCount = 10;
        Task<Result<CreateReservationResult>>[] tasks = [.. Enumerable.Range(0, parallelCount)
            .Select(_ => RunReservationAsync(data.UserId, data.TripId, contestedSeat))];

        Result<CreateReservationResult>[] results = await Task.WhenAll(tasks);

        int successCount = results.Count(r => r.IsSuccess);
        int conflictCount = results.Count(r =>
            r.IsFailure && r.Error!.Code == "Reservations.SeatsNotAvailable");

        successCount.Should().Be(1,
            "Only one parallel request should reserve the seat");
        conflictCount.Should().Be(9,
            "Nine of the remaining requests should receive SeatsNotAvailable from the pessimistic lock");

        await using AppDbContext verifyContext = fixture.CreateDbContext();
        int reservationCount = await verifyContext.Reservations.CountAsync();
        reservationCount.Should().Be(1,
            "double-booking not allowed — pessimistic lock protects against race conditions");

        TripSeat seat = await verifyContext.TripSeats.SingleAsync(ts => ts.Id == contestedSeat);
        seat.Status.Should().Be(TripSeatStatus.Reserved);
    }

    private async Task<Result<CreateReservationResult>> RunReservationAsync(
        Guid userId, Guid tripId, Guid tripSeatId)
    {
        IServiceProvider services = BuildServiceProvider(userId);
        ISender mediator = services.GetRequiredService<ISender>();

        var command = new CreateReservationCommand(
            TripId: tripId,
            TripSeatIds: [tripSeatId]);

        return await mediator.Send(command);
    }

    private IServiceProvider BuildServiceProvider(Guid userId)
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                fixture.ConnectionString,
                b => b.MigrationsAssembly("TrainBooking.Migrations")));

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString,
            })
            .Build();

        services.AddInfrastructure(configuration);

        services.AddApplication();

        services.AddLogging();

        services.AddSingleton(TimeProvider.System);

        ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(userId);
        services.AddScoped(_ => currentUser);

        return services.BuildServiceProvider();
    }
}
