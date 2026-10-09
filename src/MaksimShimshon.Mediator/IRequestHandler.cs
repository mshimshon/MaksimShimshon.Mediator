namespace MaksimShimshon.Mediator;

public interface IRequestHandler { }
public interface IRequestHandler<TCommand> : IRequestHandler
     where TCommand : IRequest
{
    Task HandleAsync(TCommand data, CancellationToken ct = default);
}

public interface IRequestHandler<TCommand, TResult> : IRequestHandler
    where TCommand : IRequest<TResult>
{
    Task<TResult> HandleAsync(TCommand data, CancellationToken ct = default);
}
