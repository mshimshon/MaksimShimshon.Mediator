using Microsoft.Extensions.DependencyInjection;

namespace MaksimShimshon.Mediator;

internal class Mediator : IMediator
{
    internal static Dictionary<Type, Type> _requestCache = new();
    internal static Dictionary<Type, Func<object, object, CancellationToken, Task<object>>> _cacheResultMethod = new();
    internal static Dictionary<Type, Func<object, object, CancellationToken, Task>> _cacheNoResultMethod = new();

    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public async Task ExecuteAsync(IRequest request, CancellationToken ct = default)
    {
        var handlerType = _requestCache[request.GetType()];
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var invoker = _cacheNoResultMethod[request.GetType()];
        await invoker(handler, request, ct);
    }



    public async Task<TResult> ExecuteAsync<TResult>(IRequest<TResult> request, CancellationToken ct = default)
    {
        var handlerType = _requestCache[request.GetType()];
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var invoker = _cacheResultMethod[request.GetType()];
        var result = await invoker(handler, request, ct);

        return (TResult)result;
    }
}