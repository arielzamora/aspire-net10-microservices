namespace Core.MediatOR.Contracts;

public interface IMediatOR
{
    Task<TResponse> Send<TResponse>(
                            IRequest<TResponse> request, 
                            CancellationToken cancellationToken
                        );
}
