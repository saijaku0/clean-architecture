using MediatR;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Abstractions.Queries;

public interface IQuery<T> : IRequest<Result<T>>;
