using BankingApp.Shared.Events;
using BankingApp.CheckingAccountService.Application.Interfaces;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Enums;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public class CreateMovementCommandHandler : IRequestHandler<CreateMovementCommand, Unit>
{
    private readonly ICheckingAccountRepository _accountRepository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IMovementRepository _movementRepository;
    private readonly ICacheService _cacheService;

    public CreateMovementCommandHandler(
        ICheckingAccountRepository accountRepository,
        IMovementRepository movementRepository,
        IKafkaProducer kafkaProducer,
        ICacheService cacheService)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _kafkaProducer = kafkaProducer;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(CreateMovementCommand request, CancellationToken cancellationToken)
    {
        CheckingAccount targetAccount;

        if (request.TargetAccountNumber.HasValue)
        {
            targetAccount = await _accountRepository.GetByAccountNumberAsync(request.TargetAccountNumber.Value)
                            ?? throw new InvalidAccountException("Conta não encontrada");
        }
        else
        {
            targetAccount = await _accountRepository.GetByIdAsync(request.RequestingAccountId)
                            ?? throw new InvalidAccountException("Conta não encontrada");
        }

        targetAccount.EnsureActive();
        var targetAccountId = targetAccount.Id;

        var movementType = request.MovementType switch
        {
            'C' => MovementType.Credit,
            'D' => MovementType.Debit,
            _ => throw new InvalidMovementTypeException("Tipo de movimento inválido. Use 'C' para crédito ou 'D' para débito")
        };

        if (movementType == MovementType.Debit && targetAccountId != request.RequestingAccountId)
            throw new InvalidMovementTypeException("Apenas o tipo crédito pode ser aceito para contas de terceiros");

        var movement = Movement.Create(targetAccountId, movementType, request.Amount);

        await _movementRepository.AddAsync(movement);

        await _cacheService.RemoveAsync($"balance:{targetAccountId}");
        await _cacheService.RemoveAsync($"account:{targetAccount.AccountNumber.Value}");

        var movementEvent = new MovementCreatedEvent
        {
            CheckingAccountId = targetAccountId,
            AccountNumber = targetAccount.AccountNumber.Value,
            MovementType = request.MovementType,
            Amount = request.Amount
        };

        await _kafkaProducer.PublishAsync("checking-account.movement", movementEvent);

        return Unit.Value;
    }
}