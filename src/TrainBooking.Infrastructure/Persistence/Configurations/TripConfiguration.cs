using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Trips;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable($"{nameof(Trip)}s");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TrainId)
            .IsRequired();

        builder.Property(t => t.OriginStation)
            .IsRequired()
            .HasMaxLength(155);

        builder.Property(t => t.DestinationStation)
            .IsRequired()
            .HasMaxLength(155);

        builder.Property(t => t.DepartureTime)
            .IsRequired();

        builder.Property(t => t.ArrivalTime)
            .IsRequired();

        builder.HasIndex(t => new { t.OriginStation, t.DestinationStation, t.DepartureTime });
    }
}
