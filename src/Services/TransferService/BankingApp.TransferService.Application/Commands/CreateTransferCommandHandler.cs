using BankingApp.Shared.Events;
using BankingApp.TransferService.Application.DTOs;
using BankingApp.TransferService.Application.Interfaces;
using BankingApp.TransferService.Application.Services;
using BankingApp.TransferService.Domain.Entities;
using BankingApp.TransferService.Domain.Exceptions;
using BankingApp.TransferService.Domain.Interfaces;
using MediatR;

namespace BankingApp.TransferService.Application.Commands;

public class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, CreateTransferResponse>
{
    private readonly ICheckingAccountApiClient _checkingAccountClient;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly TransferSagaOrchestrator _sagaOrchestrator;
    private readonly ITransferRepository _transferRepository;

    public CreateTransferCommandHandler(
        ITransferRepository transferRepository,
        ICheckingAccountApiClient checkingAccountClient,
        TransferSagaOrchestrator sagaOrchestrator,
        IKafkaProducer kafkaProducer)
    {
        _transferRepository = transferRepository;
        _checkingAccountClient = checkingAccountClient;
        _sagaOrchestrator = sagaOrchestrator;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<CreateTransferResponse> Handle(
        CreateTransferCommand request,
        CancellationToken cancellationToken)
    {
        var destinationAccountId = await _checkingAccountClient.GetAccountIdByNumberAsync(
            request.DestinationAccountNumber,
            request.JwtToken);

        if (destinationAccountId == null)
            throw new InvalidAccountException($"Conta de destino {request.DestinationAccountNumber} não encontrada");

        var transfer = Transfer.Create(
            request.OriginAccountId,
            destinationAccountId.Value,
            request.Amount);

        var sagaResult = await _sagaOrchestrator.ExecuteTransferSagaAsync(
            request.DestinationAccountNumber,
            request.Amount,
            request.JwtToken,
            transfer.Id.ToString());

        if (sagaResult.IsSuccess)
        {
            transfer.MarkAsCompleted();
            await _transferRepository.AddAsync(transfer);

            var transferEvent = new TransferCompletedEvent
            {
                TransferId = transfer.Id,
                OriginAccountId = transfer.OriginAccountId,
                DestinationAccountId = transfer.DestinationAccountId,
                Amount = transfer.Amount,
                MovementDate = transfer.MovementDate,
                IdempotencyKey = transfer.Id.ToString()
            };

            await _kafkaProducer.PublishAsync("transfer.completed", transferEvent);

            var response = new CreateTransferResponse(
                transfer.Id,
                transfer.MovementDate,
                "Completed");

            return response;
        }

        if (sagaResult.WasRolledBack)
        {
            transfer.MarkAsFailed();
            transfer.MarkAsRolledBack();
        }
        else
        {
            transfer.MarkAsFailed();
        }

        await _transferRepository.AddAsync(transfer);

        throw new TransferFailedException(sagaResult.ErrorMessage ?? "Falha ao executar transferência");
    }
}