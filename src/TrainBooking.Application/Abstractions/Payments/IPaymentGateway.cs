using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Abstractions.Payments;

public interface IPaymentGateway
{
    Task<Result<PaymentResult>> ChargeAsync(
        decimal amount,
        string paymentToken,
        decimal availableFunds,
        Guid idempotencyKey,
        CancellationToken ct = default);
}

public record PaymentResult(string TransactionId, DateTime ChargedAt);
