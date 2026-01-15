using BankingApp.Shared.Idempotency;
using BankingApp.CheckingAccountService.Application.EventHandlers;
using BankingApp.CheckingAccountService.Application.Interfaces;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Infrastructure.Caching;
using BankingApp.CheckingAccountService.Infrastructure.Database;
using BankingApp.CheckingAccountService.Infrastructure.Messaging;
using BankingApp.CheckingAccountService.Infrastructure.Repositories;
using BankingApp.CheckingAccountService.Infrastructure.Security;
using BankingApp.CheckingAccountService.Infrastructure.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.TypedHandler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BankingApp.CheckingAccountService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<DapperContext>();

        // Repositories
        services.AddScoped<ICheckingAccountRepository, CheckingAccountRepository>();
        services.AddScoped<IMovementRepository, MovementRepository>();

        // Services
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        // Redis
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddScoped<FeeAppliedEventHandler>();

        // Kafka
        var kafkaBootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        services.AddKafka(kafka => kafka
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { kafkaBootstrapServers })

                .AddProducer("checking-account-producer", producer => producer
                    .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>()))

                .AddConsumer(consumer => consumer
                    .Topics("fee.applied")
                    .WithGroupId("checking-account-service-consumer-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(5)
                    .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                    .AddMiddlewares(m => m
                        .AddDeserializer<JsonCoreDeserializer>()
                        .AddTypedHandlers(h => h
                            .AddHandler<FeeAppliedEventConsumer>())))
            )
        );

        return services;
    }
}
