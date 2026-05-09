namespace TrainBooking.Application.Abstractions.Identity;

public interface IUserResolver
{
    Task<Guid> ResolveAsync(Auth0UserInfo userInfo, CancellationToken ct);
}
