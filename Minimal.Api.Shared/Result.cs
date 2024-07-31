namespace Minimal.Api.Shared;
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool IsNotFound { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, bool isNotFound, List<string> errors) =>
        (IsSuccess, IsNotFound, Errors) = (isSuccess, isNotFound, errors ?? new List<string>());

    public static Result Fail(IEnumerable<string> errors) =>
            new Result(false, false, errors.ToList());

    public static Result Fail(string error) =>
        new Result(false, false, new List<string> { error });

    public static Result<T> Fail<T>(IEnumerable<string> errors) =>
        new Result<T>(default, false, false, errors.ToList());

    public static Result<T> Fail<T>(string error) =>
        new Result<T>(default, false, false, new List<string> { error });

    public static Result Ok() =>
        new Result(true, false, new List<string>());

    public static Result<T> Ok<T>(T value) =>
        new Result<T>(value, true, false, new List<string>());

    public static Result NotFound() =>
        new Result(false, true, new List<string>());

    public static Result<T> NotFound<T>() =>
        new Result<T>(default, false, true, new List<string>());
}

public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, bool isNotFound, List<string> errors) : base(isSuccess, isNotFound, errors) 
        => Value = value;
}

public enum FailedResultType
{
    NOT_FOUND
}