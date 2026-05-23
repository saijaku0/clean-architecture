using MediatR;
using Microsoft.AspNetCore.Mvc;
using TrainBooking.Api.Extensions;
using TrainBooking.Application.Common.Pagination;
using TrainBooking.Application.Trips.GetTripById;
using TrainBooking.Application.Trips.GetTripList;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class TripsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList(
    [FromQuery] string? origin,
    [FromQuery] string? destination,
    [FromQuery] DateTime? departureDate,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    {
        var query = new GetTripListQuery(
            origin, destination, departureDate, pageNumber, pageSize);

        Result<PagedResult<TripListItemDto>> result = await mediator.Send(query, ct);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var query = new GetTripByIdQuery(id);

        Result<GetTripByIdDto> result = await mediator.Send(query, ct);

        return result.ToActionResult();
    }
}
