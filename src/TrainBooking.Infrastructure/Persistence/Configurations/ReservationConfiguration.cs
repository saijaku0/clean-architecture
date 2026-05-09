using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Reservations;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable($"{nameof(Reservation)}s");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TripId).IsRequired();
        builder.Property(r => r.UserId).IsRequired();

        builder.Property(r => r.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.ExpiresAt).IsRequired();
        builder.Property(r => r.TripDepartureTime).IsRequired();
        builder.Property(r => r.ConfirmedAt).IsRequired(false);

        builder.HasMany(r => r.ReservationSeats)
            .WithOne()
            .HasForeignKey(rs => rs.ReservationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.ReservationSeats)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(r => new { r.UserId, r.TripId });

        builder.HasIndex(r => new { r.Status, r.ExpiresAt })
            .HasFilter("[Status] = 'Pending'");
    }
}
