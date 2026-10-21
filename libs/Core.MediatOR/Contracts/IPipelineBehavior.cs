namespace Core.MediatOR.Contracts;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
public interface IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>
{

    Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next
    );

}
