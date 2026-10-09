namespace MaksimShimshon.Mediator;

public interface IMediator
{
    Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default);
    Task ExecuteAsync(IRequest request, CancellationToken ct = default);

}
