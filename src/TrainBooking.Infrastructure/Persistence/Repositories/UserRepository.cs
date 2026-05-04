using Microsoft.EntityFrameworkCore;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Users;

namespace TrainBooking.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public Task<User?> GetByIdAsync(
        Guid Id,
        CancellationToken ct = default) =>
            _dbContext.Users.FirstOrDefaultAsync(u => u.Id == Id, ct);

    public Task<User?> GetByAuth0SubAsync(
        string Auth0Sub,
        CancellationToken ct = default) =>
            _dbContext.Users.FirstOrDefaultAsync(u => u.Auth0Sub == Auth0Sub, ct);

    public async Task AddAsync(
        User User,
        CancellationToken ct = default) =>
            await _dbContext.Users.AddAsync(User, ct);
}
