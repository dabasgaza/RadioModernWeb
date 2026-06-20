using DataAccess.Common;

namespace Radio.Tests.Helpers;

public static class AssertExtensions
{
    public static void ShouldBeSuccess(this Result result)
        => result.IsSuccess.Should().BeTrue();

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
