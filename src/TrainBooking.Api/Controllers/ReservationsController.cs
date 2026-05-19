using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainBooking.Api.Contracts.Reservations;
using TrainBooking.Api.Extensions;
using TrainBooking.Application.Reservations.Commands.ConfirmReservation;
using TrainBooking.Application.Reservations.Commands.CreateReservation;
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
        CancellationToken ct = default)
    {
        var command = new ConfirmReservationCommand(reservationId);
        Result<ConfirmReservationResult> result = await mediator.Send(command, ct);
        return result.ToActionResult();
    }
}
