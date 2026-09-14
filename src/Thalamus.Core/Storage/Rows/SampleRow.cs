using Thalamus.Core.Models;

namespace Thalamus.Core.Storage.Rows;

internal class SampleRow
{
    public string Id {get; set;} = "";
    public string ProfileId {get; set;} = "";
    public string TimestampUtc {get; set;} = "";
    public string? WindowTitle {get; set;} = "";
    public string? ProcessName {get; set;} = "";
    public long IsIdle {get; set;}
    public long IdleTime {get; set;}
    public long IsDeleted {get; set;}
    public Sample ToSample()
    {
        return new Sample(
            Guid.Parse(Id),
            Guid.Parse(ProfileId),
            DateTime.Parse(TimestampUtc),
            WindowTitle,
            ProcessName,
            IsIdle == 1,
            TimeSpan.FromSeconds(IdleTime),
            IsDeleted == 1
        );
    }
    public static SampleRow FromSample(Sample s)
    {
        return new SampleRow{
            Id = s.Id.ToString(),
            ProfileId = s.ProfileId.ToString(),
            TimestampUtc = s.TimestampUtc.ToString("o"),
            WindowTitle = s.WindowTitle,
            ProcessName = s.ProcessName,
            IsIdle = s.IsIdle ? 1 : 0,
            IdleTime = (long)Math.Round(s.IdleTime.TotalSeconds),
            IsDeleted = s.IsDeleted ? 1 : 0,
        }; 
    }
}