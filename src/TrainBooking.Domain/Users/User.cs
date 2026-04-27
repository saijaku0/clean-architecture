using TrainBooking.Domain.Common.Entities;
using TrainBooking.Domain.Common.Guards;
using TrainBooking.Domain.Common.Results;
using TrainBooking.Domain.Users.DomainEvents;
using TrainBooking.Domain.Users.Errors;

namespace TrainBooking.Domain.Users;

public class User : AggregateRoot
{
    public string Auth0Sub { get; private init; } = default!;
    public string Email { get; private set; } = default!;
    public string? FullName { get; private set; }
    public DateTime LastSyncedAt { get; private set; }

    private User() { }
    private User(
        Guid userId,
        string auth0Sub,
        string email,
        string? fullName) : base(userId)
    {
        Auth0Sub = auth0Sub;
        Email = email;
        FullName = fullName;
        LastSyncedAt = DateTime.UtcNow;

        AddDomainEvent(new UserCreatedDomainEvent(Id));
    }

    public static User Create(
        string auth0Sub,
        string email,
        string? fullName)
    {
        Guard.Against.NullOrWhiteSpace(email);
        Guard.Against.NullOrWhiteSpace(auth0Sub);

        return new User(Guid.CreateVersion7(), auth0Sub, email, fullName);
    }

    public Result UpdateProfile(
        string email,
        string? fullName)
    {
        Guard.Against.NullOrWhiteSpace(email);
        if (!IsValidEmailFormat(email))
            return UserErrors.InvalidEmailFormat(email);

        bool emailNotChanged = Email == email;
        bool fullNameNotChanged = FullName == fullName;
        if (emailNotChanged && fullNameNotChanged)
            return Result.Success();

        Email = email;
        FullName = fullName;
        LastSyncedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedDomainEvent(Id));
        return Result.Success();
    }

    // This method checks if the email format is valid. (simple check for '@' and length >= 3)
    private static bool IsValidEmailFormat(string email) =>
        email.Contains('@') && email.Length >= 3;
}
