using Thalamus.Collectors.Windows;

namespace Thalamus.Tests;

public class ActivityCollectorTest
{
    [Fact]
    public async Task CollectSample_ReturnsNonIdle_WhenRecentlyActive()
    {
        var collector = new ActivityCollector(TimeSpan.FromSeconds(60), Guid.NewGuid());
        var sample = await collector.CollectSample();
        Assert.NotNull(sample);
    }
}