using Microsoft.Data.Sqlite;
using Dapper;

namespace Thalamus.Core.Storage;

public class DbConnectionFactory
{
    private readonly string _dbPath;
    public DbConnectionFactory(string dbPath)
    {
        _dbPath = dbPath;
    }
    public async Task<SqliteConnection> OpenAsync()
    {
        var folder = Path.GetDirectoryName(_dbPath);
        if(!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }
        var connection = new SqliteConnection($"Data Source={_dbPath};");
        // Opens a connection to the profile database, creating the file and
        // applying the schema if needed.
        // The caller owns the returned connection and must dispose it.
        await connection.OpenAsync();
        await connection.ExecuteAsync(Schema.Samples);
        await connection.ExecuteAsync(Schema.SystemEvents);
        return connection;
    }
}