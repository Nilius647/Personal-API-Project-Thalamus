using Thalamus.Core.Storage;
using Thalamus.Core.Models;

namespace Thalamus.Tests;

public class RepositoryContractTests
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
    public async Task GenerateAndRead()
    {
        var profileId = Guid.NewGuid();
        var samples = CreateSamples(profileId);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        var factory = new DbConnectionFactory(path);
        var storage = new SqliteRepository(factory);
        await storage.AddSamplesAsync(samples);
        DateTime from = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
        DateTime to = new DateTime(2026, 9, 1, 16, 45, 0, DateTimeKind.Utc);
        IReadOnlyList<Sample> newSamples = await storage.GetSamplesAsync(from, to);
        Assert.Equal(5, newSamples.Count); 
        for(int i = 0; i < samples.Count; i++)
        {
            Assert.Equal(samples[i].Id, newSamples[i].Id);
        }
    }
    [Fact]
    public async Task GetInterval()
    {
        var profileId = Guid.NewGuid();
        var samples = CreateSamples(profileId);
        var storage = new MemoryRepository();
        await storage.AddSamplesAsync(samples);
        DateTime from = new DateTime(2026, 9, 1, 10, 30, 0, DateTimeKind.Utc);
        DateTime to = new DateTime(2026, 9, 1, 14, 15, 0, DateTimeKind.Utc);
        IReadOnlyList<Sample> intervalSamples = await storage.GetSamplesAsync(from, to);
        Assert.Equal(3, intervalSamples.Count);
        for(int i = 0; i < intervalSamples.Count; i++)
        {
            Assert.Equal(samples[i+1].Id, intervalSamples[i].Id);
        }
    }
    [Fact]
    public async Task EraseById_RemovesSampleFromResults()
    {
        var profileId = Guid.NewGuid();
        var samples = CreateSamples(profileId);
        var storage = new MemoryRepository();
        var deletedId = samples[0].Id;
        var from = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc);
        await storage.AddSamplesAsync(samples);
        await storage.EraseByIdAsync(deletedId, DataKind.Sample);
        var result = await storage.GetSamplesAsync(from, to);
        Assert.Equal(4, result.Count);
        Assert.DoesNotContain(result, s => s.Id == deletedId);
    }
    [Fact]
    public async Task EmptyInterval()
    {
        var profileId = Guid.NewGuid();
        var storage = new MemoryRepository();
        DateTime from = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime to = new DateTime(2026, 9, 1, 16, 45, 0, DateTimeKind.Utc);
        var result = await storage.GetSamplesAsync(from, to);
        Assert.Empty(result);
    }
}