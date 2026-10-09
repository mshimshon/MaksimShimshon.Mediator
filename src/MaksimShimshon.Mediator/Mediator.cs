namespace MaksimShimshon.Mediator;

internal class Mediator : IMediator
{
    internal static Dictionary<Type, Type> _requestCache = new();
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public async Task ExecuteAsync(IRequest request, CancellationToken ct = default)
    {
        var handlerType = _requestCache[request.GetType()];
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync((dynamic)request, ct);
    }



    public async Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default)
    {
        var handlerType = _requestCache[request.GetType()];
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return await handler.HandleAsync((dynamic)request, ct);
    }
}