using FluentValidation;

namespace TrainBooking.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Auth0Sub)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.FullName)
            .MaximumLength(255)
            .When(x => x.FullName is not null);
    }
}
