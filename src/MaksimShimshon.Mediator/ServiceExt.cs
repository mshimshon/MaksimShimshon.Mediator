using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

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
            Mediator._cacheResultMethod[requestType] = CreateResultDelegate(requestType, resultType, closedHandlerType)!;
        }
        else
        {
            // IRequestHandler<TRequest>
            var args = nonGenericIface!.GetGenericArguments();
            requestType = args[0];

            closedHandlerType = typeof(IRequestHandler<>)
                .MakeGenericType(requestType);
            Mediator._cacheNoResultMethod[requestType] = CreateNoResultDelegate(requestType, closedHandlerType);
        }
        Console.WriteLine($"Caching Mediator Handler: {requestType.Name} = {handlerType}");
        Mediator._requestCache[requestType] = handlerType;
    }

    public static Func<object, object, CancellationToken, Task<object>> CreateResultDelegate(Type requestType, Type resultType, Type genericInterfaceType)
    {
        // FIX: Look up method explicitly safely, bypassing issues with default arguments or empty lookups
        var methodInfo = genericInterfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "HandleAsync" && m.GetParameters().Length == 2)
            ?? throw new InvalidOperationException($"Could not locate HandleAsync method on {genericInterfaceType.Name}");

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var castHandler = Expression.Convert(handlerParam, genericInterfaceType);
        var castRequest = Expression.Convert(requestParam, requestType);
        var methodCall = Expression.Call(castHandler, methodInfo, castRequest, ctParam);

        // Uses your containing extension/mediator class to extract the static converter
        var conversionMethod = typeof(ServiceExt) // Make sure this matches your exact file class name!
            .GetMethod(nameof(ConvertTaskToObject), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(resultType);

        var helperCall = Expression.Call(conversionMethod, methodCall);

        return Expression.Lambda<Func<object, object, CancellationToken, Task<object>>>(
            helperCall, handlerParam, requestParam, ctParam).Compile();
    }

    public static Func<object, object, CancellationToken, Task> CreateNoResultDelegate(Type requestType, Type nonGenericInterfaceType)
    {
        // FIX: Look up method explicitly safely
        var methodInfo = nonGenericInterfaceType.GetMethods()
            .FirstOrDefault(m => m.Name == "HandleAsync" && m.GetParameters().Length == 2)
            ?? throw new InvalidOperationException($"Could not locate HandleAsync method on {nonGenericInterfaceType.Name}");

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var castHandler = Expression.Convert(handlerParam, nonGenericInterfaceType);
        var castRequest = Expression.Convert(requestParam, requestType);
        var methodCall = Expression.Call(castHandler, methodInfo, castRequest, ctParam);

        var castResult = Expression.Convert(methodCall, typeof(Task));

        return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
            castResult, handlerParam, requestParam, ctParam).Compile();
    }

    private static async Task<object?> ConvertTaskToObject<T>(Task<T> task)
    {
        return await task;
    }

}
