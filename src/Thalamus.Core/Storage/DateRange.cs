namespace Thalamus.Core.Storage;

public record DateRange
{
    public DateTime From {get;}
    public DateTime To {get;}
    public DateRange(DateTime from, DateTime to)
    {
        if(from > to)
        {
            throw new ArgumentException("From must be earlier than To", nameof(to));
        }  
        From = from;
        To = to;
    }
    public TimeSpan Duration() => To - From;
    public bool Contains(DateTime moment) => moment >= From && moment <= To;
}