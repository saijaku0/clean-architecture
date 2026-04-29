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
            modelBuilder.Entity(aggregateType).Ignore(nameof(AggregateRoot.DomainEvents));
        }

        // Retrieve all entity base types from the model
        IEnumerable<Type> entityBaseTypes = modelBuilder.Model
            .GetEntityTypes()
            .Select(e => e.ClrType)
            .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(EntityBase)));

        // Configure common properties for all entities derived from EntityBase
        foreach (Type entityType in entityBaseTypes)
        {
            EntityTypeBuilder b = modelBuilder.Entity(entityType);
            b.Property(nameof(EntityBase.Id)).ValueGeneratedNever();
            b.Property(nameof(EntityBase.CreatedAt)).IsRequired();
            b.Property(nameof(EntityBase.UpdatedAt)).IsRequired(false);
        }
    }
}
