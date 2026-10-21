namespace MasterNet.Application.Core;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public string? Error { get; set; }

    // Optional: detailed validation errors payload
    public IReadOnlyList<ValidationError>? ValidationErrors { get; set; }

    public static Result<T> Success(T value) => new Result<T>
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Failure(string error) => new Result<T>
    {
        IsSuccess = false,
        Error = error
    };

    // New: failure overload carrying a list of validation errors
    public static Result<T> Failure(IEnumerable<ValidationError> errors) => new Result<T>
    {
        IsSuccess = false,
        ValidationErrors = errors.ToList()
    };
}