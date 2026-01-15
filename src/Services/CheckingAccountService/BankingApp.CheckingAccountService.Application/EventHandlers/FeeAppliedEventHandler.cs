using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Enums;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.Shared.Events;
using BankingApp.Shared.Idempotency;

namespace BankingApp.CheckingAccountService.Application.EventHandlers;

public class FeeAppliedEventHandler
{
    private readonly ICheckingAccountRepository _accountRepository;
    private readonly IIdempotencyService _idempotencyService;
    private readonly IMovementRepository _movementRepository;

    public FeeAppliedEventHandler(
        ICheckingAccountRepository accountRepository,
        IMovementRepository movementRepository,
        IIdempotencyService idempotencyService)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _idempotencyService = idempotencyService;
    }

    public async Task HandleAsync(FeeAppliedEvent feeEvent)
    {
        if (await _idempotencyService.WasProcessedAsync(feeEvent.IdempotencyKey)) return;

        var account = await _accountRepository.GetByIdAsync(feeEvent.CheckingAccountId);
        if (account == null) throw new InvalidOperationException("Conta não encontrada para aplicação de tarifa");

        var movement = Movement.Create(
            feeEvent.CheckingAccountId,
            MovementType.Debit,
            feeEvent.Amount
        );

        await _movementRepository.AddAsync(movement);
        await _idempotencyService.MarkAsProcessedAsync(feeEvent.IdempotencyKey);
    }
}