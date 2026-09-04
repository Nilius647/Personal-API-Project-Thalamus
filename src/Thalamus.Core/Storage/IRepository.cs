using Thalamus.Core.Models;

namespace Thalamus.Core.Storage;

public interface IRepository
{
    Task AddSamplesAsync(IEnumerable<Sample> data);
    Task<IReadOnlyList<Sample>> GetSamplesAsync(DateTime from, DateTime to);
    Task AddSystemEventsAsync(IEnumerable<SystemEvent> data);
    Task<IReadOnlyList<SystemEvent>> GetSystemEventsAsync(DateTime from, DateTime to, EventKind? eventKind = null);
    Task EraseDataAsync(DateTime from, DateTime to, DataKind kind);
    Task EraseByIdAsync(Guid id, DataKind kind);
}