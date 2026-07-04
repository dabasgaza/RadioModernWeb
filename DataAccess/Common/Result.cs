// ============================================================
// Result — نتيجة العملية
// ============================================================
// المسؤولية: تعريف نتيجة العملية.
// ============================================================
namespace DataAccess.Common;

/// <summary>
/// صنف نتيجة العملية.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Success.
    /// </summary>
    public static Result Success() => new(true, null);
    /// <summary>
    /// Fail.
    /// </summary>
    public static Result Fail(string errorMessage) => new(false, errorMessage);
}

/// <summary>
/// صنف نتيجة العملية.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, string? errorMessage, T? value)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    /// <summary>
    /// Success.
    /// </summary>
    public static Result<T> Success(T value) => new(true, null, value);
    /// <summary>
    /// Fail.
    /// </summary>
    public static new Result<T> Fail(string errorMessage) => new(false, errorMessage, default);
}
