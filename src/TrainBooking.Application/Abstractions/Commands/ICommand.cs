using MediatR;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Abstractions.Commands;

public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
