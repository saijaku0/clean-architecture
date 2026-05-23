using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainBooking.Api.Contracts.Reservations;
using TrainBooking.Api.Extensions;
using TrainBooking.Application.Reservations.Commands.CancelReservation;
using TrainBooking.Application.Reservations.Commands.ConfirmReservation;
using TrainBooking.Application.Reservations.Commands.CreateReservation;
using TrainBooking.Application.Reservations.Queries.GetMyReservations;
using TrainBooking.Application.Reservations.Queries.GetReservationById;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class ReservationsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateReservationCommand(
            request.TripId,
            request.TripSeatIds);

        Result<CreateReservationResult> result = await mediator.Send(command, ct);

        return result.ToActionResult();
    }

    [HttpPost("{reservationId:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid reservationId,
        [FromBody] ConfirmReservationRequest request,
        CancellationToken ct = default)
    {
        var command = new ConfirmReservationCommand(
            reservationId,
            request.PaymentToken,
            request.AvailableFunds);

        Result<ConfirmReservationResult> result = await mediator.Send(command, ct);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var command = new CancelReservationCommand(id);
        Result result = await mediator.Send(command, ct);

        return result.ToActionResult();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyReservations(CancellationToken ct = default)
    {
        var query = new GetMyReservationsQuery();
        Result<IReadOnlyCollection<ReservationListDto>> result = await mediator.Send(query, ct);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var query = new GetReservationByIdQuery(id);
        Result<ReservationDetailsDto> result = await mediator.Send(query, ct);

        return result.ToActionResult();
    }
}
