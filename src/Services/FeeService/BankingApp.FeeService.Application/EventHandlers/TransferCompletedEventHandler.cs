using BankingApp.Shared.Events;
using BankingApp.Shared.Idempotency;
using BankingApp.FeeService.Application.Interfaces;
using BankingApp.FeeService.Application.Services;
using BankingApp.FeeService.Domain.Entities;
using BankingApp.FeeService.Domain.Interfaces;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.FeeService.Application.EventHandlers;

public class TransferCompletedEventHandler : IMessageHandler<TransferCompletedEvent>
{
    private readonly IServiceProvider _serviceProvider;

    public TransferCompletedEventHandler(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Handle(IMessageContext context, TransferCompletedEvent message)
    {
        using var scope = _serviceProvider.CreateScope();
        var feeRepository = scope.ServiceProvider.GetRequiredService<IFeeRepository>();
        var feeCalculationService = scope.ServiceProvider.GetRequiredService<FeeCalculationService>();
        var kafkaProducer = scope.ServiceProvider.GetRequiredService<IKafkaProducer>();
        var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();

        var feeIdempotencyKey = $"{message.IdempotencyKey}-fee";

        if (await idempotencyService.WasProcessedAsync(feeIdempotencyKey))
        {
            return;
        }

        var feeAmount = feeCalculationService.CalculateTransferFee();
        var fee = Fee.Create(message.OriginAccountId, feeAmount);

        await feeRepository.AddAsync(fee);

        var feeAppliedEvent = new FeeAppliedEvent
        {
            FeeId = fee.Id,
            CheckingAccountId = fee.CheckingAccountId,
            Amount = fee.Amount,
            MovementDate = fee.MovementDate,
            IdempotencyKey = feeIdempotencyKey
        };

        await kafkaProducer.PublishAsync("fee.applied", feeAppliedEvent);
        await idempotencyService.MarkAsProcessedAsync(feeIdempotencyKey);
    }
}