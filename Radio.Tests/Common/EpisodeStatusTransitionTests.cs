using DataAccess.Common;
using DataAccess.Services;

namespace Radio.Tests.Common;

public class EpisodeStatusTransitionTests
{
    [Theory]
    [InlineData(EpisodeStatusValues.Planned, EpisodeStatusValues.Executed, true)]
    [InlineData(EpisodeStatusValues.Planned, EpisodeStatusValues.Cancelled, true)]
    [InlineData(EpisodeStatusValues.Executed, EpisodeStatusValues.Published, true)]
    [InlineData(EpisodeStatusValues.Executed, EpisodeStatusValues.Cancelled, true)]
    [InlineData(EpisodeStatusValues.Published, EpisodeStatusValues.WebsitePublished, true)]
    [InlineData(EpisodeStatusValues.Published, EpisodeStatusValues.Cancelled, true)]
    [InlineData(EpisodeStatusValues.WebsitePublished, EpisodeStatusValues.Cancelled, true)]
    [InlineData(EpisodeStatusValues.Planned, EpisodeStatusValues.Published, false)]
    [InlineData(EpisodeStatusValues.Planned, EpisodeStatusValues.WebsitePublished, false)]
    [InlineData(EpisodeStatusValues.Executed, EpisodeStatusValues.WebsitePublished, false)]
    [InlineData(EpisodeStatusValues.Cancelled, EpisodeStatusValues.Planned, false)]
    [InlineData(EpisodeStatusValues.Cancelled, EpisodeStatusValues.Executed, false)]
    [InlineData(EpisodeStatusValues.Cancelled, EpisodeStatusValues.Published, false)]
    [InlineData(EpisodeStatusValues.Cancelled, EpisodeStatusValues.WebsitePublished, false)]
    [InlineData(EpisodeStatusValues.Planned, EpisodeStatusValues.Planned, false)]
    [InlineData(EpisodeStatusValues.Executed, EpisodeStatusValues.Executed, false)]
    public void IsValid_ReturnsExpected(byte from, byte to, bool expected)
    {
        Assert.Equal(expected, EpisodeStatusTransition.IsValid(from, to));
    }

    [Fact]
    public void GetAllowedTransitions_FromPlanned_ReturnsExecutedAndCancelled()
    {
        var allowed = EpisodeStatusTransition.GetAllowedTransitions(EpisodeStatusValues.Planned);
        Assert.Equal(2, allowed.Count);
        Assert.Contains(EpisodeStatusValues.Executed, allowed);
        Assert.Contains(EpisodeStatusValues.Cancelled, allowed);
    }

    [Fact]
    public void GetAllowedTransitions_FromExecuted_ReturnsPublishedAndCancelled()
    {
        var allowed = EpisodeStatusTransition.GetAllowedTransitions(EpisodeStatusValues.Executed);
        Assert.Equal(2, allowed.Count);
        Assert.Contains(EpisodeStatusValues.Published, allowed);
        Assert.Contains(EpisodeStatusValues.Cancelled, allowed);
    }

    [Fact]
    public void GetAllowedTransitions_FromCancelled_ReturnsEmpty()
    {
        var allowed = EpisodeStatusTransition.GetAllowedTransitions(EpisodeStatusValues.Cancelled);
        Assert.Empty(allowed);
    }

    [Fact]
    public void GetAllowedTransitions_FromUnknown_ReturnsEmpty()
    {
        var allowed = EpisodeStatusTransition.GetAllowedTransitions(99);
        Assert.Empty(allowed);
    }

    [Fact]
    public void IsValid_SameStatus_ReturnsFalse()
    {
        Assert.False(EpisodeStatusTransition.IsValid(EpisodeStatusValues.Planned, EpisodeStatusValues.Planned));
        Assert.False(EpisodeStatusTransition.IsValid(EpisodeStatusValues.Cancelled, EpisodeStatusValues.Cancelled));
    }
}
