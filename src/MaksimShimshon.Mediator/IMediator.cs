namespace MaksimShimshon.Mediator;

public interface IMediator
{
    Task ExecuteAsync(IRequest request, CancellationToken ct = default);
    Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default);
}
