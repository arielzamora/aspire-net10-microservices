using Core.MediatOR.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Core.MediatOR;

public class MediatOR : IMediatOR
{
    private readonly IServiceProvider _provider;

    public MediatOR(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request, 
        CancellationToken cancellationToken
    )
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();

        // Resolve the handler for the closed generic IRequestHandler<requestType, TResponse>
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _provider.GetRequiredService(handlerType);

        // Build the handler delegate using reflection (works with internal handlers and explicit impls)
        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            var handleMethod = handlerType.GetMethod("Handle")!;
            // invoke Task<TResponse> Handle(TRequest request, CancellationToken ct)
            var taskObj = handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;
            return (Task<TResponse>)taskObj;
        };

        // Resolve pipeline behaviors for the current closed generic IPipelineBehavior<requestType, TResponse>
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _provider.GetServices(behaviorType).Reverse().ToList();

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () =>
            {
                var handleMethod = behaviorType.GetMethod("Handle")!;
                // invoke Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
                var taskObj = handleMethod.Invoke(behavior, new object[] { request, cancellationToken, next })!;
                return (Task<TResponse>)taskObj;
            };
        }

        return await handlerDelegate();
    }
}
