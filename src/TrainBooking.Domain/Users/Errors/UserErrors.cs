using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Domain.Users.Errors;

public static class UserErrors
{
    public static Error DuplicateEmail(string email) =>
            Error.Conflict("user.duplicate_email", $"User with email {email} already exists.");
    public static Error InvalidEmailFormat(string email) =>
            Error.Validation("user.invalid_email_format", $"Email {email} is not in a valid format.");
}
