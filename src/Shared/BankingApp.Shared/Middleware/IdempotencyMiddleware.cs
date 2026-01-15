using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankingApp.Shared.Middleware;

public class IdempotencyMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "POST" &&
            context.Request.Method != "PUT" &&
            context.Request.Method != "PATCH")
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["Idempotency-Key"].FirstOrDefault();

        if (string.IsNullOrEmpty(idempotencyKey))
        {
            await _next(context);
            return;
        }

        var cachedResponse = await GetCachedResponseAsync(idempotencyKey);
        if (cachedResponse != null)
        {
            context.Response.StatusCode = cachedResponse.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse.Body);
            return;
        }

        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            context.Response.Body = originalBodyStream;

            if (responseBody.Length > 0)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                    await CacheResponseAsync(idempotencyKey, context.Response.StatusCode, responseText);

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }

    private async Task<CachedResponse?> GetCachedResponseAsync(string key)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqliteConnection(connectionString);

        var sql = "SELECT request, result FROM idempotency WHERE idempotency_key = @Key";
        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Key = key });

        if (row == null)
            return null;

        return new CachedResponse
        {
            StatusCode = int.Parse((string)row.request),
            Body = (string)row.result
        };
    }

    private async Task CacheResponseAsync(string key, int statusCode, string body)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqliteConnection(connectionString);

        var sql = @"
            INSERT OR REPLACE INTO idempotency (idempotency_key, request, result)
            VALUES (@Key, @Request, @Result)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Key = key,
            Request = statusCode.ToString(),
            Result = body
        });
    }

    private class CachedResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
    }
}
