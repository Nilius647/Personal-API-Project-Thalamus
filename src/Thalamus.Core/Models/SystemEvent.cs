namespace Thalamus.Core.Models;

public record SystemEvent(
    Guid Id,
    Guid ProfileId,
    DateTime TimestampUtc,
    EventKind Kind,
    bool IsDeleted = false
);