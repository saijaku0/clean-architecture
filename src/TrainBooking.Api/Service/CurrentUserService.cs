using TrainBooking.Api.Constants;
using TrainBooking.Application.Abstractions.Identity;

namespace TrainBooking.Api.Service;

internal sealed class CurrentUserService(IHttpContextAccessor accessor)
    : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            object? value = accessor.HttpContext?.Items[HttpContextItemKeys.UserId];
            return value as Guid?;
        }
    }
}
