using Thalamus.Core.Storage;
using Thalamus.Core.Models;

namespace Thalamus.Tests;

public class ProfileManagerTests
{
    private static List<Sample> CreateSamples(Guid profileId)
    {
        return new List<Sample>
        {
            new(Guid.NewGuid(), profileId, new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc),
                "Program.cs - Thalamus", "devenv", false, TimeSpan.Zero, false),

            new(Guid.NewGuid(), profileId, new DateTime(2026, 9, 1, 10, 30, 0, DateTimeKind.Utc),
                "Stack Overflow - Chrome", "chrome", false, TimeSpan.Zero, false),

            new(Guid.NewGuid(), profileId, new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                null, null, true, TimeSpan.FromMinutes(15), false),

            new(Guid.NewGuid(), profileId, new DateTime(2026, 9, 1, 14, 15, 0, DateTimeKind.Utc),
                "Sample.cs - Thalamus", "devenv", false, TimeSpan.Zero, false),

            new(Guid.NewGuid(), profileId, new DateTime(2026, 9, 1, 16, 45, 0, DateTimeKind.Utc),
                "Spotify", "spotify", false, TimeSpan.Zero, false)
        };
    }
    [Fact]
    public async Task ProfileAppearsInList()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"thalamus-{Guid.NewGuid()}");
        var manager = new ProfileManager(folder);
        var created = await manager.CreateProfileAsync("test");
        var list = await manager.ProfileListAsync();
        Assert.Contains(created, list);
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(folder, true);
    }
    [Fact]
    public async Task TwoProfiles()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"thalamus-{Guid.NewGuid()}");
        var manager = new ProfileManager(folder);
        var created1 = await manager.CreateProfileAsync("test1");
        var created2 = await manager.CreateProfileAsync("test2");
        var repo1 = await manager.GetProfileRepositoryAsync(created1.Id);
        var samples = CreateSamples(created1.Id);
        await repo1.AddSamplesAsync(samples);
        var repo2 = await manager.GetProfileRepositoryAsync(created2.Id);
        var  samplesList = await repo2.GetSamplesAsync(new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 16, 45, 0, DateTimeKind.Utc));
        Assert.Empty(samplesList);
        Assert.Equal(created1, await manager.GetCurrentProfileAsync());
        await manager.SetCurrentProfileAsync(created2.Id);
        Assert.Equal(created2, await manager.GetCurrentProfileAsync());
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(folder, true);
    }
}