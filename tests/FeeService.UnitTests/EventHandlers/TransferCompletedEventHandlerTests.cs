using BankingApp.FeeService.Application.EventHandlers;
using BankingApp.FeeService.Application.Interfaces;
using BankingApp.FeeService.Application.Services;
using BankingApp.FeeService.Domain.Entities;
using BankingApp.FeeService.Domain.Interfaces;
using BankingApp.Shared.Events;
using BankingApp.Shared.Idempotency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BankingApp.FeeService.UnitTests.EventHandlers;

public class TransferCompletedEventHandlerTests
{
    [Fact]
    public async Task Handle_ValidEvent_ShouldCreateFeeAndPublishEvent()
    {
        var mockFeeRepository = new Mock<IFeeRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockIdempotencyService = new Mock<IIdempotencyService>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfiguration.Setup(c => c["FeeConfig:TransferFeeAmount"]).Returns("2.00");
        var feeCalculationService = new FeeCalculationService(mockConfiguration.Object);

        mockIdempotencyService.Setup(s => s.WasProcessedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockFeeRepository.Object);
        services.AddScoped(_ => feeCalculationService);
        services.AddScoped(_ => mockKafkaProducer.Object);
        services.AddScoped(_ => mockIdempotencyService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new TransferCompletedEventHandler(serviceProvider);
        var transferEvent = new TransferCompletedEvent
        {
            TransferId = Guid.NewGuid(),
            OriginAccountId = Guid.NewGuid(),
            DestinationAccountId = Guid.NewGuid(),
            Amount = 500m,
            IdempotencyKey = "transfer-123"
        };

        await handler.Handle(null!, transferEvent);

        mockFeeRepository.Verify(r => r.AddAsync(It.Is<Fee>(f =>
            f.CheckingAccountId == transferEvent.OriginAccountId &&
            f.Amount == 2.00m)), Times.Once);
        mockKafkaProducer.Verify(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        mockIdempotencyService.Verify(i => i.MarkAsProcessedAsync("transfer-123-fee", null, null), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyProcessedEvent_ShouldReturnEarly()
    {
        var mockFeeRepository = new Mock<IFeeRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockIdempotencyService = new Mock<IIdempotencyService>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfiguration.Setup(c => c["FeeConfig:TransferFeeAmount"]).Returns("2.00");
        var feeCalculationService = new FeeCalculationService(mockConfiguration.Object);

        mockIdempotencyService.Setup(s => s.WasProcessedAsync("transfer-123-fee"))
            .ReturnsAsync(true);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockFeeRepository.Object);
        services.AddScoped(_ => feeCalculationService);
        services.AddScoped(_ => mockKafkaProducer.Object);
        services.AddScoped(_ => mockIdempotencyService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new TransferCompletedEventHandler(serviceProvider);
        var transferEvent = new TransferCompletedEvent
        {
            TransferId = Guid.NewGuid(),
            OriginAccountId = Guid.NewGuid(),
            DestinationAccountId = Guid.NewGuid(),
            Amount = 500m,
            IdempotencyKey = "transfer-123"
        };

        await handler.Handle(null!, transferEvent);

        mockFeeRepository.Verify(r => r.AddAsync(It.IsAny<Fee>()), Times.Never);
        mockKafkaProducer.Verify(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeriveCorrectIdempotencyKey()
    {
        var mockFeeRepository = new Mock<IFeeRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockIdempotencyService = new Mock<IIdempotencyService>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockConfiguration.Setup(c => c["FeeConfig:TransferFeeAmount"]).Returns("2.00");
        var feeCalculationService = new FeeCalculationService(mockConfiguration.Object);

        mockIdempotencyService.Setup(s => s.WasProcessedAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockFeeRepository.Object);
        services.AddScoped(_ => feeCalculationService);
        services.AddScoped(_ => mockKafkaProducer.Object);
        services.AddScoped(_ => mockIdempotencyService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var handler = new TransferCompletedEventHandler(serviceProvider);
        var transferEvent = new TransferCompletedEvent
        {
            TransferId = Guid.NewGuid(),
            OriginAccountId = Guid.NewGuid(),
            DestinationAccountId = Guid.NewGuid(),
            Amount = 500m,
            IdempotencyKey = "my-custom-key-456"
        };

        await handler.Handle(null!, transferEvent);

        mockIdempotencyService.Verify(i => i.WasProcessedAsync("my-custom-key-456-fee"), Times.Once);
        mockIdempotencyService.Verify(i => i.MarkAsProcessedAsync("my-custom-key-456-fee", null, null), Times.Once);
    }
}
