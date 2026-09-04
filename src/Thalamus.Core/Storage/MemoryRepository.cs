using Thalamus.Core.Models;

namespace Thalamus.Core.Storage;

public class MemoryRepository : IRepository
{
    private readonly List<Sample> _samples = new();
    private readonly List<SystemEvent> _systemEvents = new();
    private void EraseSamplesInRange(DateRange range)
    {
        for(int i = 0; i < _samples.Count; i++)
        {
            if(range.Contains(_samples[i].TimestampUtc))
            {
                _samples[i] = _samples[i] with {IsDeleted = true};
            }
        }
    }
    private void EraseEventSystemsInRange(DateRange range)
    {
        for(int j = 0; j < _systemEvents.Count; j++)
        {
            if(range.Contains(_systemEvents[j].TimestampUtc))
            {
                _systemEvents[j] = _systemEvents[j] with {IsDeleted = true};
            }    
        }
    }   
    private void EraseSampleById(Guid id)
    {
        var index_s = _samples.FindIndex(s => s.Id == id);
        if(index_s >= 0)
        {
            _samples[index_s] = _samples[index_s] with {IsDeleted = true};
        }
    }
    private void EraseEventSystemById(Guid id)
    {
        var index_se = _systemEvents.FindIndex(s => s.Id == id);
        if(index_se >= 0)
        {
            _systemEvents[index_se] = _systemEvents[index_se] with {IsDeleted = true};
        }
    }

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
    public Task EraseDataAsync(DateTime from, DateTime to, DataKind kind)
    {
        var range  = new DateRange(from, to);
        switch (kind)
        {
            case DataKind.All:
                EraseSamplesInRange(range);
                EraseEventSystemsInRange(range);
                break;
            case DataKind.Sample:
                EraseSamplesInRange(range);
                break;
            case DataKind.SystemEvent:
                EraseEventSystemsInRange(range);
                break;
        }
        return Task.CompletedTask;
    }
    public Task EraseByIdAsync(Guid id, DataKind kind)
    {
        switch (kind)
        {
            case DataKind.All:
                EraseSampleById(id);
                EraseEventSystemById(id);
                break;
            case DataKind.Sample:
                EraseSampleById(id);
                break;
            case DataKind.SystemEvent:
                EraseEventSystemById(id);
                break;
        }
        return Task.CompletedTask;
    }
}