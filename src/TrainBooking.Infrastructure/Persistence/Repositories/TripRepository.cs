using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Common.Pagination;
using TrainBooking.Application.Trips.GetTripList;
using TrainBooking.Domain.Trips;
using TrainBooking.Domain.TripSeats.Enums;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class TripRepository(AppDbContext dbContext) : ITripRepository
{
    public async Task<Trip?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default) =>
            await dbContext.Trips.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<PagedResult<TripListItemDto>> GetListAsync(
        string? origin,
        string? destination,
        DateTime? departureDate,
        int pageNumber,
        int pageSize,
        DateTime now,
        CancellationToken ct = default)
    {
        IQueryable<Trip> query = dbContext.Trips
            .Where(t => t.DepartureTime > now);

        if (!string.IsNullOrWhiteSpace(origin))
            query = query.Where(t => t.OriginStation == origin);

        if (!string.IsNullOrWhiteSpace(destination))
            query = query.Where(t => t.DestinationStation == destination);

        if (departureDate.HasValue)
        {
            DateTime dayStart = departureDate.Value.Date;
            DateTime dayEnd = dayStart.AddDays(1);
            query = query.Where(t =>
                t.DepartureTime >= dayStart && t.DepartureTime < dayEnd);
        }

        int totalCount = await query.CountAsync(ct);

        int skip = (pageNumber - 1) * pageSize;

        List<TripListItemDto> items = await query
            .OrderBy(t => t.DepartureTime)
            .Skip(skip)
            .Take(pageSize)
            .Select(t => new TripListItemDto(
                t.Id,
                t.OriginStation,
                t.DestinationStation,
                t.DepartureTime,
                t.ArrivalTime,
                dbContext.TripSeats
                    .Count(ts => ts.TripId == t.Id && ts.Status == TripSeatStatus.Available),
                dbContext.TripSeats
                    .Where(ts => ts.TripId == t.Id && ts.Status == TripSeatStatus.Available)
                    .Min(ts => (decimal?)ts.Price) ?? 0m))
            .ToListAsync(ct);

        return new PagedResult<TripListItemDto>(
            items, pageNumber, pageSize, totalCount);
    }

}
