using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Reservations;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class ReservationSeatConfiguration : IEntityTypeConfiguration<ReservationSeat>
{
    public void Configure(EntityTypeBuilder<ReservationSeat> builder)
    {
        builder.ToTable($"{nameof(ReservationSeat)}s");
        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.TripSeatId).IsRequired();
        builder.HasIndex(rs => rs.TripSeatId);

        builder.Property(rs => rs.PriceSnapshot)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
