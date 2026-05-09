using TrainBooking.Api.Constants;
using TrainBooking.Application.Abstractions.Identity;

namespace TrainBooking.Api.Middleware;

internal sealed class Auth0UserProvisioningMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUserResolver resolver)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        string? sub = context.User.FindFirst("sub")?.Value;
        string? email = context.User.FindFirst("https://api.trainbooking.app/email")?.Value;
        string? name = context.User.FindFirst("https://api.trainbooking.app/name")?.Value;

        if (sub is null || email is null)
            throw new InvalidOperationException("Missing required claims in JWT");

        Guid userId = await resolver.ResolveAsync(
            new Auth0UserInfo(sub, email, name),
            context.RequestAborted);

        context.Items[HttpContextItemKeys.UserId] = userId;

        await next(context);
    }
}
