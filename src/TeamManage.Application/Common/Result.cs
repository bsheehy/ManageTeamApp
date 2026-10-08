namespace TeamManage.Application.Common;

/// <summary>
/// Represents the outcome of an operation without relying on exceptions for
/// expected failure cases (e.g. validation, not found, forbidden).
/// </summary>
public class Result
{
    public bool Succeeded { get; }

    public string? Error { get; }

    protected Result(bool succeeded, string? error)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);
}

/// <summary>
/// A <see cref="Result"/> that also carries a return value on success.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; }

    protected Result(bool succeeded, T? data, string? error) : base(succeeded, error)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, null);

    public static new Result<T> Failure(string error) => new(false, default, error);
}
