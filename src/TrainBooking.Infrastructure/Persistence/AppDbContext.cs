using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Reservations;
using TrainBooking.Domain.Trains;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats;
using TrainBooking.Domain.Users;

namespace TrainBooking.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Train> Trains { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<TripSeat> TripSeats { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Retrieve all aggregate types from the model and configure their properties
        IEnumerable<Type> aggregateTypes = modelBuilder.Model
            .GetEntityTypes()
            .Select(e => e.ClrType)
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(AggregateRoot)));

        // Configure properties of aggregates. Traverse the aggregate tree, ignore DomainEvents and configure Id as ValueGeneratedNever
        foreach (Type aggregateType in aggregateTypes)
        {
            EntityTypeBuilder aggregateTypeBuilder = modelBuilder.Entity(aggregateType);
            aggregateTypeBuilder.Ignore(nameof(AggregateRoot.DomainEvents));
            aggregateTypeBuilder.Property(nameof(AggregateRoot.Id)).ValueGeneratedNever();
        }
    }
}
