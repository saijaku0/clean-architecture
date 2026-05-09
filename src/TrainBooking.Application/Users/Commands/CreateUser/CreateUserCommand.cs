using TrainBooking.Application.Abstractions.Commands;

namespace TrainBooking.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Auth0Sub,
    string Email,
    string? FullName) : ICommand<Guid>;
