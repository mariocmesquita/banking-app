using System.Net;
using System.Text.Json;
using BankingApp.TransferService.Domain.Exceptions;
using FluentValidation;

namespace BankingApp.TransferService.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        object message;

        switch (exception)
        {
            case InvalidAccountException:
                statusCode = HttpStatusCode.BadRequest;
                message = new
                {
                    errorCode = "INVALID_ACCOUNT",
                    message = exception.Message,
                    status = (int)HttpStatusCode.BadRequest
                };
                break;

            case TransferFailedException:
                statusCode = HttpStatusCode.BadRequest;
                message = new
                {
                    errorCode = "TRANSFER_FAILED",
                    message = exception.Message,
                    status = (int)HttpStatusCode.BadRequest
                };
                break;

            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = new
                {
                    errorCode = "VALIDATION_ERROR",
                    message = "Erro de validação",
                    status = (int)HttpStatusCode.BadRequest,
                    errors = validationEx.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        message = e.ErrorMessage
                    })
                };
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = new
                {
                    errorCode = "USER_UNAUTHORIZED",
                    message = exception.Message,
                    status = (int)HttpStatusCode.Unauthorized
                };
                break;

            case InvalidOperationException when exception.Message.Contains("não encontrada"):
                statusCode = HttpStatusCode.NotFound;
                message = new
                {
                    errorCode = "RESOURCE_NOT_FOUND",
                    message = exception.Message,
                    status = (int)HttpStatusCode.NotFound
                };
                break;

            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = new
                {
                    errorCode = "INVALID_VALUE",
                    message = exception.Message,
                    status = (int)HttpStatusCode.BadRequest
                };
                break;

            case HttpRequestException:
                statusCode = HttpStatusCode.BadGateway;
                message = new
                {
                    errorCode = "EXTERNAL_SERVICE_ERROR",
                    message = "Erro ao comunicar com serviço externo",
                    details = exception.Message,
                    status = (int)HttpStatusCode.BadGateway
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = new
                {
                    errorCode = "INTERNAL_ERROR",
                    message = "Erro interno do servidor",
                    status = (int)HttpStatusCode.InternalServerError
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(message));
    }
}