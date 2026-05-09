using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TrainBooking.Domain.Trains.ValueObjects;

namespace TrainBooking.Infrastructure.Persistence.Conversions;

public sealed class SeatClassConverter : ValueConverter<SeatClass, string>
{
    public SeatClassConverter()
        : base(
            seatClass => seatClass.Name,
            value => SeatClass.FromName(value))
    {
    }
}
