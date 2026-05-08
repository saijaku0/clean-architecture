using MediatR;
using Microsoft.Extensions.Caching.Memory;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Application.Users.Commands.CreateUser;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Users;

namespace TrainBooking.Infrastructure.Services;

internal sealed class UserResolver(
    IMemoryCache cache,
    IUserRepository userRepository,
    ISender sender) : IUserResolver
{
    public async Task<Guid> ResolveAsync(Auth0UserInfo userInfo, CancellationToken ct)
    {
        if (cache.TryGetValue(userInfo.Sub, out Guid cachedId))
            return cachedId;

        User? existing = await userRepository.GetByAuth0SubAsync(userInfo.Sub, ct);
        if (existing is not null)
        {
            cache.Set(userInfo.Sub, existing.Id, TimeSpan.FromMinutes(20));
            return existing.Id;
        }

        Result<Guid> result = await sender.Send(new CreateUserCommand(userInfo.Sub, userInfo.Email, userInfo.FullName), ct);
        if (result.IsFailure)
            throw new InvalidOperationException("Cannot create new user");

        cache.Set(userInfo.Sub, result.Value, TimeSpan.FromMinutes(20));
        return result.Value;
    }
}
