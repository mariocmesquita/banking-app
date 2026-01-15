using BankingApp.Shared.Idempotency;
using BankingApp.FeeService.Application.EventHandlers;
using BankingApp.FeeService.Application.Interfaces;
using BankingApp.FeeService.Domain.Interfaces;
using BankingApp.FeeService.Infrastructure.Database;
using BankingApp.FeeService.Infrastructure.Messaging;
using BankingApp.FeeService.Infrastructure.Repositories;
using BankingApp.FeeService.Infrastructure.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.FeeService.Infrastructure;

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
        services.AddScoped<IFeeRepository, FeeRepository>();

        // Services
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        // Handlers
        services.AddSingleton<TransferCompletedEventHandler>();

        // Kafka
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { kafkaBootstrapServers })

                .AddProducer("fee-producer", producer => producer
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))

                .AddConsumer(consumer => consumer
                    .Topics("transfer.completed")
                    .WithGroupId("fee-service-consumer-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(5)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .AddHandler<TransferCompletedEventHandler>())))
            )
        );

        return services;
    }
}