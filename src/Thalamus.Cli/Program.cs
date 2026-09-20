using Thalamus.Collectors.Windows;
using Thalamus.Core.Models;
using Thalamus.Core.Storage;

if (args.Length == 0)
{
    Console.WriteLine("Usage: thalamus <collect|list|profiles>");
    return;
}
switch (args[0])
{
    case "collect":
    {
        var PManager1 = new ProfileManager(AppPaths.BaseFolder);
        var profile = await PManager1.GetCurrentProfileAsync();
        if (profile is null)
            profile = await PManager1.CreateProfileAsync("Default");
        var repository = await PManager1.GetProfileRepositoryAsync(profile.Id);
        var activityCollector = new ActivityCollector(TimeSpan.FromSeconds(60), profile.Id);
        var eventLogCollector = new EventLogCollector(profile.Id);
        using var cts = new CancellationTokenSource();
        var events = await eventLogCollector.CollectSystemEvents(cts.Token);
        if (events.Count > 0)
            await repository.AddSystemEventsAsync(events);
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        var buffer = new List<Sample>();
        Console.WriteLine($"Collecting for profile '{profile.Name}'. Press Ctrl+C to stop.");
        while (!cts.Token.IsCancellationRequested)
        {
            var sample = await activityCollector.CollectSample(cts.Token);
            if (sample is not null)
            {
                buffer.Add(sample);
                Console.WriteLine($"{sample.ProcessName ?? "(idle)"}  idle={sample.IdleTime.TotalSeconds:F0}s");
            }
            if (buffer.Count >= 10)
            {
                await repository.AddSamplesAsync(buffer);
                buffer.Clear();
            }
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
        if (buffer.Count > 0)
            await repository.AddSamplesAsync(buffer);

        Console.WriteLine("Stopped. Data saved.");
        break;
    }
    case "list":
    {
        var PManager2 = new ProfileManager(AppPaths.BaseFolder);
        var profile = await PManager2.GetCurrentProfileAsync();
        if (profile is null)
        {
            Console.WriteLine("No profile found.");
            break;
        }
        var repo = await PManager2.GetProfileRepositoryAsync(profile.Id);
        var from = DateTime.UtcNow.AddHours(-24);
        var to = DateTime.UtcNow;
        var samples = await repo.GetSamplesAsync(from, to);
        Console.WriteLine($"{samples.Count} sample(s) in the last 24 hours:\n");
        foreach (var sample in samples)
            Console.WriteLine(sample);
        break;
    }
    case "profiles":
    {
        var PManager3 = new ProfileManager(AppPaths.BaseFolder);
        var profiles = await PManager3.ProfileListAsync();
        foreach (var profile in profiles)
            Console.WriteLine($"{profile.Name}  ({profile.Id})");
        break;
    }
    default:
        Console.WriteLine($"Unknown command: {args[0]}");
        break;
}