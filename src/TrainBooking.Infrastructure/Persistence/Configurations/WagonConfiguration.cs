using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Trains;
using TrainBooking.Infrastructure.Persistence.Conversions;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class WagonConfiguration : IEntityTypeConfiguration<Wagon>
{
    public void Configure(EntityTypeBuilder<Wagon> builder)
    {
        builder.ToTable($"{nameof(Wagon)}s");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.Number)
            .IsRequired();

        builder.Property(w => w.Class)
            .HasConversion<SeatClassConverter>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(w => w.Seats)
            .WithOne()
            .HasForeignKey(w => w.WagonId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Seats)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
