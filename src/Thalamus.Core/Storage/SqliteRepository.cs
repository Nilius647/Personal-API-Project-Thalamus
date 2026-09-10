using Dapper;
using Thalamus.Core.Models;
using Thalamus.Core.Storage.Rows;

namespace Thalamus.Core.Storage;


public class SqliteRepository : IRepository
{
    private readonly DbConnectionFactory _factory;
    public SqliteRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }
    public async Task AddSamplesAsync(IEnumerable<Sample> data)
    {
        using var conn = await _factory.OpenAsync();
        const string sql1 = "INSERT INTO samples (Id, ProfileId, TimestampUtc, WindowTitle, ProcessName, IsIdle, IdleTime, IsDeleted) VALUES (@Id, @ProfileId, @TimestampUtc, @WindowTitle, @ProcessName, @IsIdle, @IdleTime, @IsDeleted)";
        var rows = data.Select(SampleRow.FromSample).ToList();
        await conn.ExecuteAsync(sql1, rows);
    }
    public async Task<IReadOnlyList<Sample>> GetSamplesAsync(DateTime from, DateTime to)
    {
        using var conn = await _factory.OpenAsync();
        const string sql2 = "SELECT * FROM samples WHERE IsDeleted = 0 AND TimestampUtc >= @From AND TimestampUtc <= @To";
        var parameters = new {From = from.ToString("o"), To = to.ToString("o")};
        var rows = await conn.QueryAsync<SampleRow>(sql2, parameters);
        return rows.Select(r => r.ToSample()).ToList();
    }
    public async Task AddSystemEventsAsync(IEnumerable<SystemEvent> data)
    {
        using var conn = await _factory.OpenAsync();
        const string sql3 = "INSERT INTO systemEvents (Id, ProfileId, TimestampUtc, Kind, IsDeleted) VALUES (@Id, @ProfileId, @TimestampUtc, @Kind, @IsDeleted)";
        var rows = data.Select(SystemEventRow.FromSystemEvent).ToList();
        await conn.ExecuteAsync(sql3, rows);
    }
    public async Task<IReadOnlyList<SystemEvent>> GetSystemEventsAsync(DateTime from, DateTime to, EventKind? eventKind = null)
    {
        using var conn = await _factory.OpenAsync();
        var parameters = new {From = from.ToString("o"), To = to.ToString("o"), EventKind = eventKind};
        if(eventKind == null)
        {
            const string sql4 = "SELECT * FROM systemEvents WHERE IsDeleted = 0 AND TimestampUtc >= @From AND TimestampUtc <= @To";
            var rows = await conn.QueryAsync<SystemEventRow>(sql4, parameters);
            return rows.Select(r => r.ToSystemEvent()).ToList();
        }else{
            const string sql4 = "SELECT * FROM systemEvents WHERE IsDeleted = 0 AND TimestampUtc >= @From AND TimestampUtc <= @To AND Kind = @EventKind";
            var rows = await conn.QueryAsync<SystemEventRow>(sql4, parameters);
            return rows.Select(r => r.ToSystemEvent()).ToList();
        }
    }
    public async Task EraseDataAsync(DateTime from, DateTime to, DataKind kind)
    {
        using var conn = await _factory.OpenAsync();
        var parameters = new {From = from.ToString("o"), To = to.ToString("o")};
        if(kind == DataKind.Sample)
        {
           const string sql5 = "UPDATE samples SET IsDeleted = 1 WHERE TimestampUtc >= @From AND TimestampUtc <= @To";
        await conn.ExecuteAsync(sql5, parameters);
        }else if(kind == DataKind.SystemEvent){
            const string sql5 = "UPDATE systemEvents SET IsDeleted = 1 WHERE TimestampUtc >= @From AND TimestampUtc <= @To";
            await conn.ExecuteAsync(sql5, parameters);
        }else if(kind == DataKind.All){
            const string sql5_1 = "UPDATE systemEvents SET IsDeleted = 1 WHERE TimestampUtc >= @From AND TimestampUtc <= @To";
            await conn.ExecuteAsync(sql5_1, parameters);
            const string sql5_2 = "UPDATE samples SET IsDeleted = 1 WHERE TimestampUtc >= @From AND TimestampUtc <= @To";
            await conn.ExecuteAsync(sql5_2, parameters);
        }
    }
    public async Task EraseByIdAsync(Guid Id, DataKind kind)
    {
        using var conn = await _factory.OpenAsync();
        var parameter = new{Id};
        if(kind == DataKind.Sample)
        {
            const string sql5 = "UPDATE samples SET IsDeleted = 1 WHERE Id = @Id";
            await conn.ExecuteAsync(sql5, parameter);
        }else if(kind == DataKind.SystemEvent){
            const string sql5 = "UPDATE systemEvents SET IsDeleted = 1 WHERE Id = @Id";
            await conn.ExecuteAsync(sql5, parameter);
        }else if(kind == DataKind.All){
            const string sql5_1 = "UPDATE systemEvents SET IsDeleted = 1 WHERE Id = @Id";
            await conn.ExecuteAsync(sql5_1, parameter);
            const string sql5_2 = "UPDATE samples SET IsDeleted = 1 WHERE Id = @Id";
            await conn.ExecuteAsync(sql5_2, parameter);
        }
    }
}