using FluentValidation;
using FluentValidation.Results;
using TrainBooking.Application.Abstractions.Commands;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Users;

namespace TrainBooking.Application.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IValidator<CreateUserCommand> validator,
    IUnitOfWork unitOfWork) : CommandHandler<CreateUserCommand, Guid>(unitOfWork)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IValidator<CreateUserCommand> _validator = validator;
    protected override async Task<Result<Guid>> HandleAsync(
        CreateUserCommand req,
        CancellationToken ct)
    {
        ValidationResult validation = await _validator.ValidateAsync(req, ct);
        if (!validation.IsValid)
        {
            string message = string.Join(
                "; ",
                validation.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            return Error.Validation("Users.Validation", message);
        }

        User? existing = await _userRepository.GetByAuth0SubAsync(req.Auth0Sub, ct);
        if (existing is not null)
            return existing.Id;

        var user = User.Create(req.Auth0Sub, req.Email, req.FullName);
        await _userRepository.AddAsync(user, ct);

        return user.Id;
    }
}
