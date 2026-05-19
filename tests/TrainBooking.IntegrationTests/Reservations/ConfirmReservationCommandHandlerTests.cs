using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using TrainBooking.Application;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Reservations.Commands.ConfirmReservation;
using TrainBooking.Application.Reservations.Commands.CreateReservation;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Reservations.Enums;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.TripSeats.Enums;
using TrainBooking.Infrastructure;
using TrainBooking.Infrastructure.Persistence;
using TrainBooking.IntegrationTests.Infrastructure;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrainBooking.IntegrationTests.Reservations;

[Collection("IntegrationTests")]
public class ConfirmReservationCommandHandlerTests(DatabaseFixture fixture)
{
    [Fact]
    public async Task Handle_WithValidRequest_ConfirmsReservationAndMarksSeatsSold()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData scenarioData = await TestData.SeedReservationScenarioAsync(fixture);
        IServiceProvider serviceProvider = BuildServiceProvider(scenarioData.UserId);
        ISender mediator = serviceProvider.GetRequiredService<ISender>();

        var createCommand = new CreateReservationCommand(
            TripId: scenarioData.TripId,
            TripSeatIds: [.. scenarioData.TripSeatIds.Take(2)]);

        Result<CreateReservationResult> createResult = await mediator.Send(createCommand);
        createResult.IsSuccess.Should().BeTrue("setup must succeed before testing Confirm");

        Guid reservationId = createResult.Value.ReservationId;

        var confirmCommand = new ConfirmReservationCommand(ReservationId: reservationId);
        Result<ConfirmReservationResult> confirmResult = await mediator.Send(confirmCommand);

        confirmResult.IsSuccess.Should().BeTrue();
        confirmResult.Value.ReservationId.Should().Be(reservationId);
        confirmResult.Value.TripId.Should().Be(scenarioData.TripId);
        confirmResult.Value.TotalPrice.Should().Be(200m);
        confirmResult.Value.ConfirmedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await using AppDbContext verifyContext = fixture.CreateDbContext();

        Reservation reservation = await verifyContext.Reservations
            .SingleAsync(r => r.Id == reservationId);

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
        reservation.ConfirmedAt.Should().NotBeNull();

        List<TripSeat> soldSeats = await verifyContext.TripSeats
            .Where(ts => scenarioData.TripSeatIds.Take(2).Contains(ts.Id))
            .ToListAsync();

        soldSeats.Should().HaveCount(2);
        soldSeats.Should().AllSatisfy(s => s.Status.Should().Be(TripSeatStatus.Sold));
    }
    [Fact]
    public async Task Handle_WhenCallerIsNotOwner_ReturnsForbidden()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        IServiceProvider servicesUserA = BuildServiceProvider(data.UserId);
        ISender mediatorA = servicesUserA.GetRequiredService<ISender>();

        var createCommand = new CreateReservationCommand(
            TripId: data.TripId,
            TripSeatIds: [.. data.TripSeatIds.Take(1)]);

        Result<CreateReservationResult> createResult = await mediatorA.Send(createCommand);
        createResult.IsSuccess.Should().BeTrue();

        Guid reservationId = createResult.Value.ReservationId;

        var userIdB = Guid.NewGuid();
        IServiceProvider servicesUserB = BuildServiceProvider(userIdB);
        ISender mediatorB = servicesUserB.GetRequiredService<ISender>();

        var confirmCommand = new ConfirmReservationCommand(reservationId);
        Result<ConfirmReservationResult> result = await mediatorB.Send(confirmCommand);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Forbidden);

        await using AppDbContext verifyContext = fixture.CreateDbContext();

        Reservation reservation = await verifyContext.Reservations
            .SingleAsync(r => r.Id == reservationId);

        reservation.Status.Should().Be(ReservationStatus.Pending,
            "Reservation must remain Pending — Confirm should not have succeeded");

        List<TripSeat> seats = await verifyContext.TripSeats
            .Where(ts => data.TripSeatIds.Take(1).Contains(ts.Id))
            .ToListAsync();

        seats.Should().AllSatisfy(s => s.Status.Should().Be(TripSeatStatus.Reserved));
    }
    [Fact]
    public async Task Handle_WhenReservationNotFound_ReturnsNotFoundError()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        IServiceProvider services = BuildServiceProvider(data.UserId);
        ISender mediator = services.GetRequiredService<ISender>();

        var nonExistentReservationId = Guid.NewGuid();

        var command = new ConfirmReservationCommand(nonExistentReservationId);
        Result<ConfirmReservationResult> result = await mediator.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Reservations.ConfirmReservation.ReservationNotFound");
    }
    [Fact]
    public async Task Handle_WhenReservationAlreadyConfirmed_ReturnsConflictError()
    {
        await TestData.CleanDatabaseAsync(fixture);
        ReservationScenarioData data = await TestData.SeedReservationScenarioAsync(fixture);

        IServiceProvider services = BuildServiceProvider(data.UserId);
        ISender mediator = services.GetRequiredService<ISender>();

        var createCommand = new CreateReservationCommand(
            TripId: data.TripId,
            TripSeatIds: [.. data.TripSeatIds.Take(1)]);

        Result<CreateReservationResult> createResult = await mediator.Send(createCommand);
        createResult.IsSuccess.Should().BeTrue();

        Guid reservationId = createResult.Value.ReservationId;

        var firstConfirm = new ConfirmReservationCommand(reservationId);
        Result<ConfirmReservationResult> firstResult = await mediator.Send(firstConfirm);
        firstResult.IsSuccess.Should().BeTrue("first Confirm must succeed");

        var secondConfirm = new ConfirmReservationCommand(reservationId);
        Result<ConfirmReservationResult> secondResult = await mediator.Send(secondConfirm);

        secondResult.IsFailure.Should().BeTrue();
        secondResult.Error!.Type.Should().Be(ErrorType.Conflict);
        secondResult.Error.Code.Should().Be("reservation.already_confirmed");
    }

    // Could be extracted to a common test base class
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
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton(TimeProvider.System);

        ICurrentUserService currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(userId);
        services.AddScoped(_ => currentUser);

        return services.BuildServiceProvider();
    }
}
