namespace PokeBuilder.Server.Models.Common;

/// <summary>
/// Non-generic service result for operations that don't return data (e.g. Delete).
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; protected set; }
    public string? Error { get; protected set; }

    /// <summary>HTTP status code hint for the controller layer.</summary>
    public int StatusCode { get; protected set; } = 200;

    public static ServiceResult Ok() =>
        new() { IsSuccess = true };

    public static ServiceResult Fail(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}

/// <summary>
/// Generic service result that carries a data payload on success.
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Ok(T data) =>
        new() { IsSuccess = true, Data = data };

    public new static ServiceResult<T> Fail(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };
}
