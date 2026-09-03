using Thalamus.Core.Models;

namespace Thalamus.Core.Storage;

public class MemoryRepository : IRepository
{
    private readonly List<Sample> _samples = new();
    private readonly List<SystemEvent> _systemEvents = new();

    public Task AddSamplesAsync(IEnumerable<Sample> data)
    {
        _samples.AddRange(data);
        return Task.CompletedTask;
    }
    public Task<IReadOnlyList<Sample>> GetSamplesAsync(DateTime from, DateTime to)
    {
        var range  = new DateRange(from, to);
        var filtered = _samples.Where(s => !s.IsDeleted && range.Contains(s.TimestampUtc)).ToList();
        return Task.FromResult<IReadOnlyList<Sample>>(filtered);
    }
    public Task AddSystemEventsAsync(IEnumerable<SystemEvent> data)
    {
        _systemEvents.AddRange(data);
        return Task.CompletedTask;
    }
    public Task<IReadOnlyList<SystemEvent>> GetSystemEventsAsync(DateTime from, DateTime to, EventKind? eventKind = null)
    {
        var range  = new DateRange(from, to);
        var filtered = _systemEvents.Where(s => !s.IsDeleted && range.Contains(s.TimestampUtc) && (s.Kind == eventKind || eventKind == null)).ToList();
        return Task.FromResult<IReadOnlyList<SystemEvent>>(filtered);
    }
    public Task EraseDataAsync(DateTime from, DateTime to, bool eraseSamples)
    {
        var range  = new DateRange(from, to);
        if(eraseSamples)
        {
            for(int i = 0; i < _samples.Count(); i++)
            {
                if(range.Contains(_samples[i].TimestampUtc))
                {
                    _samples[i] = _samples[i] with {IsDeleted = true};
                }
            }
        }
        else
        {
            for(int j = 0; j < _systemEvents.Count(); j++)
            {
                if(range.Contains(_systemEvents[j].TimestampUtc))
                {
                    _systemEvents[j] = _systemEvents[j] with {IsDeleted = true};
                }
            }
        }
        return Task.CompletedTask;
    }
    public Task EraseByIdAsync(Guid id, bool eraseSamples)
    {
        if(eraseSamples)
        {
            var index = _samples.FindIndex(s => s.Id == id);
            if(index >= 0)
            {
                _samples[index] = _samples[index] with {IsDeleted = true};
            }
        }
        else
        {
            var index = _systemEvents.FindIndex(s => s.Id == id);
            if(index >= 0)
            {
                _systemEvents[index] = _systemEvents[index] with {IsDeleted = true};
            }
        }
        return Task.CompletedTask;
    }
}