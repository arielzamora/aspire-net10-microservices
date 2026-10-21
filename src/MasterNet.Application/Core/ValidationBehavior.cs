using System.Reflection;
using Core.MediatOR.Contracts;

namespace MasterNet.Application.Core;
 
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            var errors = _validators
                .SelectMany(v => v.Validate(request))
                .ToList();

            if (errors.Count > 0)
            {
                // Si el response es Result<T>, devolvemos un Result<T>.Failure(errors)
                if (typeof(TResponse).IsGenericType &&
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var innerType = typeof(TResponse).GetGenericArguments()[0];
                    var closedResult = typeof(Result<>).MakeGenericType(innerType);
                    var failureMethod = closedResult.GetMethod(
                        "Failure",
                        BindingFlags.Public | BindingFlags.Static,
                        new[] { typeof(IEnumerable<ValidationError>) }
                    );

                    var failure = failureMethod!.Invoke(null, new object[] { errors });
                    return (TResponse)failure!;
                }

                throw ValidationException.From(errors);
            }
        }

        return await next();
    }
}