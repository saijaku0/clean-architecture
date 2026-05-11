using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainBooking.Application.Abstractions.Identity;

namespace TrainBooking.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromServices] ICurrentUserService userService) =>
        Ok(new { userId = userService.UserId });
}
