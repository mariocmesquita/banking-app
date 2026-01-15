using System.Text.Json;
using BankingApp.Shared.Idempotency;
using BankingApp.TransferService.Infrastructure.Database;
using Dapper;

namespace BankingApp.TransferService.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly DapperContext _context;

    public IdempotencyService(DapperContext context)
    {
        _context = context;
    }

    public async Task<bool> WasProcessedAsync(string idempotencyKey)
    {
        using var connection = _context.CreateConnection();

        var sql = "SELECT COUNT(1) FROM idempotency WHERE idempotency_key = @Key";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Key = idempotencyKey });

        return count > 0;
    }

    public async Task MarkAsProcessedAsync(string idempotencyKey, string? request = null, string? result = null)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT OR REPLACE INTO idempotency (idempotency_key, request, result)
            VALUES (@Key, @Request, @Result)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Key = idempotencyKey,
            Request = request ?? string.Empty,
            Result = result ?? string.Empty
        });
    }

    public async Task<T?> GetCachedResultAsync<T>(string idempotencyKey) where T : class
    {
        using var connection = _context.CreateConnection();

        var sql = "SELECT result FROM idempotency WHERE idempotency_key = @Key";
        var result = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Key = idempotencyKey });

        if (string.IsNullOrEmpty(result))
            return null;

        return JsonSerializer.Deserialize<T>(result);
    }

    public async Task CacheResultAsync<T>(string idempotencyKey, T result, string? request = null) where T : class
    {
        var json = JsonSerializer.Serialize(result);
        await MarkAsProcessedAsync(idempotencyKey, request, json);
    }
}