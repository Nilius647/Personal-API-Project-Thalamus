using Thalamus.Core.Collection;
using Thalamus.Core.Models;
using System.Diagnostics.Eventing.Reader;

namespace Thalamus.Collectors.Windows;

public class EventLogCollector : ISystemEventCollector
{
    private readonly Guid _profileId;
    public EventLogCollector(Guid profileId)
    {
        _profileId = profileId;
    }
    public string Name => "windows-eventlog";
    public bool IsAvailable => OperatingSystem.IsWindows();
    public Task<IReadOnlyList<SystemEvent>> CollectSystemEvents(CancellationToken token = default)
    {
        string query = "*[System/EventID=6005 or System/EventID=6006 or System/EventID=42 or System/EventID=107]";
        var eventsQuery = new EventLogQuery("System", PathType.LogName, query);
        using var reader = new EventLogReader(eventsQuery);
        List<SystemEvent> list = [];
        for(var record = reader.ReadEvent(); record != null; record = reader.ReadEvent())
        {
            if (record.TimeCreated is null)
            {
                record.Dispose();
                continue;
            }
            int eventId = record.Id;
            var timestamp = record.TimeCreated.Value.ToUniversalTime();
            EventKind kind = eventId switch
            {
                6005 => EventKind.Boot,
                6006 => EventKind.Shutdown,
                42 => EventKind.Sleep,
                107 => EventKind.Wake,
                _ => throw new InvalidOperationException($"Unexpected event ID: {eventId}")
            };
            var systemEvent = new SystemEvent(
                Guid.NewGuid(),
                _profileId,
                timestamp,
                kind,
                false
            );
            list.Add(systemEvent);
            record.Dispose();
        }
        return Task.FromResult<IReadOnlyList<SystemEvent>>(list);
    }
}