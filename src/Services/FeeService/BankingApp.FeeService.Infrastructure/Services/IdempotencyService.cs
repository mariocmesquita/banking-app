using BankingApp.FeeService.Infrastructure.Database;
using BankingApp.Shared.Idempotency;
using Dapper;

namespace BankingApp.FeeService.Infrastructure.Services;

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
            INSERT OR REPLACE INTO idempotency (idempotency_key, processed_at)
            VALUES (@Key, @ProcessedAt)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Key = idempotencyKey,
            ProcessedAt = DateTime.UtcNow.ToString("O")
        });
    }

    public async Task<T?> GetCachedResultAsync<T>(string idempotencyKey) where T : class
    {
        return null;
    }

    public async Task CacheResultAsync<T>(string idempotencyKey, T result, string? request = null) where T : class
    {
        await MarkAsProcessedAsync(idempotencyKey);
    }
}