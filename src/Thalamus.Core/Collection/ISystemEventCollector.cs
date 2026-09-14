using Thalamus.Core.Models;

namespace Thalamus.Core.Collection;

public interface ISystemEventCollector
{
    string Name {get;}
    bool IsAvailable {get;}
    Task<IReadOnlyList<SystemEvent>> CollectSystemEvents(CancellationToken token = default);
}