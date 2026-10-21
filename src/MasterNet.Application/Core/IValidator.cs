namespace MasterNet.Application.Core;

public interface IValidator<TRequest>
{
    IEnumerable<ValidationError> Validate(TRequest request);
}
