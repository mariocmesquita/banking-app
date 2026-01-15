namespace BankingApp.Shared.Idempotency;

public interface IIdempotencyService
{
    Task<bool> WasProcessedAsync(string idempotencyKey);
    Task MarkAsProcessedAsync(string idempotencyKey, string? request = null, string? result = null);
    Task<T?> GetCachedResultAsync<T>(string idempotencyKey) where T : class;
    Task CacheResultAsync<T>(string idempotencyKey, T result, string? request = null) where T : class;
}