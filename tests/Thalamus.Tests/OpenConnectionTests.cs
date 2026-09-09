using Thalamus.Core.Storage;

namespace Thalamus.Tests;

public class OpenConnectionTest
{   
    [Fact]
    public async Task OpenConnectionWorks(){
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        DbConnectionFactory factory = new(path);
        using (var conn = await factory.OpenAsync())
        {
            Assert.True(File.Exists(path));
        }
    }
}