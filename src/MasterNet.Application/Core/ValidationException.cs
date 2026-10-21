
namespace MasterNet.Application.Core;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
       : base("Validation failed") => Errors = errors;

    public static ValidationException From(IEnumerable<ValidationError> errors) =>
        new(errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

    //public ValidationException(IEnumerable<ValidationError> errors)
    //{
    //    Errors = errors;
    //}

    //public IEnumerable<ValidationError> Errors { get; }

}