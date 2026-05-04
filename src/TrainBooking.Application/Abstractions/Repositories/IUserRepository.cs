using TrainBooking.Domain.Users;

namespace TrainBooking.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task<User?> GetByAuth0SubAsync(string Auth0Sub, CancellationToken ct = default);
    Task AddAsync(User User, CancellationToken ct = default);
}
