// ============================================================
// AssertExtensions — التأكيدات
// ============================================================
// المسؤولية: تعريف التأكيدات.
// ============================================================
using DataAccess.Common;

namespace Radio.Tests.Helpers;

/// <summary>
/// صنف التأكيدات.
/// </summary>
public static class AssertExtensions
{
    /// <summary>
    /// Should Be Success.
    /// </summary>
    public static void ShouldBeSuccess(this Result result)
        => result.IsSuccess.Should().BeTrue();

    /// <summary>
    /// Should Be Failure.
    /// </summary>
    public static void ShouldBeFailure(this Result result, string? expectedMessage = null)
    {
        result.IsSuccess.Should().BeFalse();
        if (expectedMessage != null)
            result.ErrorMessage.Should().Contain(expectedMessage);
    }

    public static T ShouldBeSuccess<T>(this Result<T> result)
    {
        result.IsSuccess.Should().BeTrue();
        return result.Value!;
    }

    public static void ShouldBeFailure<T>(this Result<T> result, string? expectedMessage = null)
    {
        result.IsSuccess.Should().BeFalse();
        if (expectedMessage != null)
            result.ErrorMessage.Should().Contain(expectedMessage);
    }
}
