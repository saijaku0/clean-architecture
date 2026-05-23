using TrainBooking.Application.Abstractions.Payments;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Infrastructure.Payments;

/// <summary>
/// Mock implementation of a payment gateway for demonstration purposes.
/// In a real application, this would integrate with an actual payment processing service.
/// </summary>
internal sealed class PaymentGateway(TimeProvider timeProvider) : IPaymentGateway
{
    public async Task<Result<PaymentResult>> ChargeAsync(
        decimal amount,
        string paymentToken,
        decimal availableFunds,
        Guid idempotencyKey,
        CancellationToken ct = default)
    {
        await Task.Delay(50, ct);

        return paymentToken switch
        {
            "tok_visa_4242" =>
                availableFunds >= amount
                    ? Result<PaymentResult>.Success(new PaymentResult(
                        Guid.NewGuid().ToString(),
                        timeProvider.GetUtcNow().UtcDateTime
                    ))
                    : Error.Conflict("payment.insufficient_funds", "Insufficient funds."),
            "tok_decline" =>
                Error.Conflict("payment.declined", "Payment was declined."),
            "tok_error" =>
                Error.Internal("payment.gateway_error", "An error occurred with the payment gateway."),
            _ => Error.BadRequest("payment.invalid_token", "Invalid payment token."),
        };
    }

}
