using System.Diagnostics;

namespace BankingApp.Gateway.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingCorrelationId) &&
            !string.IsNullOrWhiteSpace(existingCorrelationId))
            correlationId = existingCorrelationId!;
        else
            correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();

        context.Response.Headers.Append(CorrelationIdHeader, correlationId);
        context.Items[CorrelationIdHeader] = correlationId;

        if (Activity.Current != null) Activity.Current.AddTag("correlation_id", correlationId);

        await _next(context);
    }
}