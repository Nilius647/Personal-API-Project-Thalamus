using Thalamus.Core.Models;

namespace Thalamus.Core.Storage.Rows;

internal class SystemEventRow
{
    public string Id {get; set;} = "";
    public string ProfileId {get; set;} = "";
    public string TimestampUtc {get; set;} = "";
    public long Kind {get; set;}
    public long IsDeleted {get; set;}
    public SystemEvent ToSystemEvent()
    {
        return new SystemEvent(
            Guid.Parse(Id),
            Guid.Parse(ProfileId),
            DateTime.Parse(TimestampUtc),
            (EventKind)Kind,
            IsDeleted == 1
        );
    }
    public static SystemEventRow FromSystemEvent(SystemEvent s)
    {
        return new SystemEventRow{
            Id = s.Id.ToString(),
            ProfileId = s.ProfileId.ToString(),
            TimestampUtc = s.TimestampUtc.ToString("o"),
            Kind = (long)s.Kind,
            IsDeleted = s.IsDeleted ? 1 : 0,
        }; 
    }
}