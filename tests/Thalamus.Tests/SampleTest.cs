using Thalamus.Core.Models;

namespace Thalamus.Tests;

public class SampleTests{
    [Fact]
    public void Sample_HasGeneratedId()
    {
        var ProfileId = Guid.NewGuid();
        var s = new Sample(
        Id: Guid.NewGuid(),
        ProfileId: ProfileId,
        TimestampUtc: DateTime.UtcNow,
        WindowTitle: "Sample.cs - Thalamus",
        ProcessName: "devenv",
        IsIdle: false,
        IdleTime: TimeSpan.Zero,
        IsDeleted: false
        );

        Assert.NotEqual(Guid.Empty, s.Id);
        Assert.False(s.IsDeleted);
    }
}