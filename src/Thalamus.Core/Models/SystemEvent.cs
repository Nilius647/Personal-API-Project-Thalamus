namespace Thalamus.Core.Models;

public record SystemEvent(
    Guid Id,
    Guid ProfileId,
    DateTime TimestampUTC,
    string Kind,
    bool IsDeleted = false
);