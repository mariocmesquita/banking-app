using BankingApp.Shared.Idempotency;
using BankingApp.TransferService.Application.Interfaces;
using BankingApp.TransferService.Domain.Interfaces;
using BankingApp.TransferService.Infrastructure.Database;
using BankingApp.TransferService.Infrastructure.HttpClients;
using BankingApp.TransferService.Infrastructure.Messaging;
using BankingApp.TransferService.Infrastructure.Repositories;
using BankingApp.TransferService.Infrastructure.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace BankingApp.TransferService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Dapper
        services.AddSingleton<DapperContext>();
        services.AddSingleton<DatabaseInitializer>();

        // Repositories
        services.AddScoped<ITransferRepository, TransferRepository>();

        // Services
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        var checkingAccountServiceUrl = configuration["CheckingAccountService:BaseUrl"]
                                        ?? "http://localhost:5001";

        services.AddHttpClient<ICheckingAccountApiClient, CheckingAccountApiClient>(client =>
            {
                client.BaseAddress = new Uri(checkingAccountServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        // Kafka
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { kafkaBootstrapServers })

                .AddProducer("transfer-producer", producer => producer
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))
            )
        );

        return services;
    }

    // Retry
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                3,
                retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                    TimeSpan.FromMilliseconds(new Random().Next(0, 1000)),
                (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine(
                        $"Tentativa {retryCount} após {timespan.TotalSeconds}s devido a: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                });
    }

    // Circuit breaker
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                (outcome, duration) => { Console.WriteLine($"Circuit breaker aberto por {duration.TotalSeconds}s"); },
                () => { Console.WriteLine("Circuit breaker fechado"); });
    }
}