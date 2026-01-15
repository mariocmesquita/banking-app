using BankingApp.Shared.Events;
using BankingApp.CheckingAccountService.Application.EventHandlers;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.CheckingAccountService.Infrastructure.Messaging;

public class FeeAppliedEventConsumer : KafkaFlow.TypedHandler.IMessageHandler<FeeAppliedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FeeAppliedEventConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Handle(IMessageContext context, FeeAppliedEvent message)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<FeeAppliedEventHandler>();
        await handler.HandleAsync(message);
    }
}