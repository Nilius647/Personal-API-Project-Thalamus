using Thalamus.Core.Models;

namespace Thalamus.Core.Collection;

public interface ISampleCollector
{
    string Name {get;}
    bool IsAvailable {get;}
    Task<Sample?> CollectSample(CancellationToken token = default);
}