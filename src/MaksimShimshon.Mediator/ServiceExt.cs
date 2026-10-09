namespace MaksimShimshon.Mediator;

public static class ServiceExt
{
    private static IServiceCollection? _allServices;
    internal static bool Processed { get; private set; }
    public static void AddMediatorService(this IServiceCollection services)
    {
        _allServices = services;
        services.AddScoped<IMediator, Mediator>();

    }
    public static void UseMediator(this WebApplication app, IServiceCollection services)
    {
        _allServices = services;
        app.UseMediator();
    }
    public static void UseMediator(this WebApplication app)
    {
        if (_allServices == default)
            throw new ArgumentException("You must call AddMediatorServices before.");
        foreach (var sd in _allServices)
        {
            var implType = sd.ImplementationType ?? sd.ImplementationInstance?.GetType() ?? sd.ServiceType;

            if (implType == null)
                continue;

            if (!typeof(IRequestHandler).IsAssignableFrom(implType))
                continue;
            Console.WriteLine($"Found Mediator Handler: {implType.Name}");
            CacheMediatorService(implType);
        }
    }


    private static void CacheMediatorService(Type handlerType)
    {

        // Find IRequestHandler<TRequest, TResult>
        var genericIface = handlerType.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

        // Find IRequestHandler<TRequest>
        var nonGenericIface = handlerType.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<>));

        if (genericIface == null && nonGenericIface == null)
            throw new InvalidOperationException($"{handlerType} does not implement IRequestHandler<> or IRequestHandler<,>");

        Type requestType;
        Type closedHandlerType;

        if (genericIface != null)
        {
            // IRequestHandler<TRequest, TResult>
            var args = genericIface.GetGenericArguments();
            requestType = args[0];
            var resultType = args[1];

            closedHandlerType = typeof(IRequestHandler<,>)
                .MakeGenericType(requestType, resultType);
        }
        else
        {
            // IRequestHandler<TRequest>
            var args = nonGenericIface!.GetGenericArguments();
            requestType = args[0];

            closedHandlerType = typeof(IRequestHandler<>)
                .MakeGenericType(requestType);
        }
        Mediator._requestCache[requestType] = closedHandlerType;
    }


}
