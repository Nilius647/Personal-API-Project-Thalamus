using Thalamus.Core;
using Xunit;

namespace Thalamus.Tests;

public class SmokeTests
{
    [Fact]
    public void BuildInfo_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(AppInfo.BuildInfo()));
    }
}