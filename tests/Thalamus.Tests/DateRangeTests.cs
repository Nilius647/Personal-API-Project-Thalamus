using Thalamus.Core.Storage;

namespace Thalamus.Tests;

public class DateRangeTests
{
    [Fact]
    public void DateRange_Works()
    {
        var from   = new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc);
        var to     = new DateTime(2026, 9, 1, 17, 0, 0, DateTimeKind.Utc);
        var range  = new DateRange(from, to);
        var inside = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        Assert.Throws<ArgumentException>(() => new DateRange(to, from));
        Assert.Equal(TimeSpan.FromHours(9), range.Duration());
        Assert.True(range.Contains(inside));
    }
}