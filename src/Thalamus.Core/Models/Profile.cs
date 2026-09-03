namespace Thalamus.Core.Models;

public record Profile(
    Guid Id,
    string Name,
    DateTime TimestampUtc
);