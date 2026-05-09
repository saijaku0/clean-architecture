using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrainBooking.Domain.Trains;

namespace TrainBooking.Infrastructure.Persistence.Configurations;

internal sealed class TrainConfiguration : IEntityTypeConfiguration<Train>
{
    public void Configure(EntityTypeBuilder<Train> builder)
    {
        builder.ToTable($"{nameof(Train)}s");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasMany(t => t.Wagons)
            .WithOne()
            .HasForeignKey(t => t.TrainId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Wagons)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
