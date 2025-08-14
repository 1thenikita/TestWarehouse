using Microsoft.AspNetCore.Mvc;

namespace TestWarehouse.Application.Results;

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    protected Result(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new Result(true, null);
    public static Result Fail(string errorMessage) => new Result(false, errorMessage);

    public override string ToString() =>
        Success ? "Success" : $"Error: {ErrorMessage}";
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool success, T? value, string? errorMessage)
        : base(success, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new Result<T>(true, value, null);
    public new static Result<T> Fail(string errorMessage) => new Result<T>(false, default, errorMessage);

    public override string ToString() =>
        Success ? $"Success: {Value}" : $"Error: {ErrorMessage}";
}

public static class ResultExtensions
{
    public static ActionResult ToActionResult(this Result result)
    {
        if (result.Success)
            return new OkResult();

        return new BadRequestObjectResult(new { error = result.ErrorMessage });
    }

    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.Success)
            return new OkObjectResult(result.Value);

        return new BadRequestObjectResult(new { error = result.ErrorMessage });
    }
}