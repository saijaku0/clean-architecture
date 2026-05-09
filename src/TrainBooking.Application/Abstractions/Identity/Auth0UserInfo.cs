namespace TrainBooking.Application.Abstractions.Identity;

public sealed record Auth0UserInfo(string Sub, string Email, string? FullName);
