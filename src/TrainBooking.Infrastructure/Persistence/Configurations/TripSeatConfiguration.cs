using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.TripSeats;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class TripSeatConfiguration : IEntityTypeConfiguration<TripSeat>
{
    public void Configure(EntityTypeBuilder<TripSeat> builder)
    {
        builder.ToTable($"{nameof(TripSeat)}s");

        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.TripId)
            .IsRequired();

        builder.Property(ts => ts.SeatId)
            .IsRequired();

        builder.Property(ts => ts.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ts => ts.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(ts => new { ts.TripId, ts.SeatId })
            .IsUnique();
        builder.HasIndex(ts => new { ts.TripId, ts.Status })
            .HasFilter("[Status] = 'Available'")
            .HasDatabaseName("IX_TripSeats_TripId_Available_Filtered");
    }
}
