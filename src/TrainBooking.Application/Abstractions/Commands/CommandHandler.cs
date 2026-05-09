using MediatR;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Application.Abstractions.Commands;

/// <summary>
/// Represents a base command handler for commands that do not return a value.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <param name="unitOfWork">The unit of work used to persist changes.</param>
public abstract class CommandHandler<TCommand>(IUnitOfWork unitOfWork) : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> Handle(TCommand request, CancellationToken ct)
    {
        Result result = await HandleAsync(request, ct);
        if (result.IsSuccess)
            await _unitOfWork.CommitAsync(ct);
        return result;
    }

    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the execution result.
    /// </returns>
    protected abstract Task<Result> HandleAsync(TCommand request, CancellationToken ct);
}

/// <summary>
/// Represents a base command handler for commands that return a value.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <param name="unitOfWork">The unit of work used to persist changes.</param>
public abstract class CommandHandler<TCommand, TResponse>(IUnitOfWork unitOfWork) : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<TResponse>> Handle(TCommand request, CancellationToken ct)
    {
        Result<TResponse> result = await HandleAsync(request, ct);
        if (result.IsSuccess)
            await _unitOfWork.CommitAsync(ct);
        return result;
    }

    /// <summary>
    /// Processes the given command asynchronously and returns a result.
    /// </summary>
    /// <param name="request">The command instance.</param>
    /// <param name="ct">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="Result{TResponse}"/> representing the outcome of the operation.
    /// </returns>
    protected abstract Task<Result<TResponse>> HandleAsync(TCommand request, CancellationToken ct);
}
