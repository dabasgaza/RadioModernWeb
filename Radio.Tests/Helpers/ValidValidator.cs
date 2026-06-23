using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Radio.Tests.Helpers;

public static class ValidValidator
{
    public static IValidator<T> Create<T>()
    {
        var mock = new Mock<IValidator<T>>();
        mock.Setup(x => x.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock.Object;
    }
}
