namespace Thalamus.Core.Models;

public record Sample(
    Guid Id,
    Guid ProfileId,
    DateTime TimestampUtc,
    string? WindowTitle,
    string? ProcessName,
    bool IsIdle,
    TimeSpan IdleTime,
    bool IsDeleted
);