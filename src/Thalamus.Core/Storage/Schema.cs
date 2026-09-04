namespace Thalamus.Core.Storage;

public static class Schema
{
    public const string Samples = """
        CREATE TABLE IF NOT EXISTS samples (
            Id TEXT PRIMARY KEY,
            ProfileId TEXT NOT NULL,
            TimestampUtc TEXT NOT NULL,
            WindowTitle TEXT,
            ProcessName TEXT,
            IsIdle INTEGER NOT NULL,
            IdleTime INTEGER NOT NULL,
            IsDeleted INTEGER NOT NULL
            );
        CREATE INDEX IF NOT EXISTS idx_samples_timestamp ON samples(TimestampUtc);
        """;

    public const string SystemEvents = """
        CREATE TABLE IF NOT EXISTS systemEvents (
            Id TEXT PRIMARY KEY,
            ProfileId TEXT NOT NULL,
            TimestampUtc TEXT NOT NULL,
            Kind INTEGER NOT NULL,
            IsDeleted INTEGER NOT NULL
            );
        CREATE INDEX IF NOT EXISTS idx_systemEvents_timestamp ON systemEvents(TimestampUtc);
        """;
}