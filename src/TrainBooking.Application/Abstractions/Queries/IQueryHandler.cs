using MediatR;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Abstractions.Queries;

// Queries are read-only, so no base class is needed - they don't share state-mutation infrastructure.

public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
