using BankingApp.TransferService.Application.Interfaces;
using BankingApp.TransferService.Application.Models;

namespace BankingApp.TransferService.Application.Services;

public class TransferSagaOrchestrator
{
    private readonly ICheckingAccountApiClient _checkingAccountClient;

    public TransferSagaOrchestrator(
        ICheckingAccountApiClient checkingAccountClient)
    {
        _checkingAccountClient = checkingAccountClient;
    }

    public async Task<SagaResult> ExecuteTransferSagaAsync(
        long destinationAccountNumber,
        decimal amount,
        string jwtToken,
        string baseIdempotencyKey)
    {
        var debitIdempotencyKey = $"{baseIdempotencyKey}-debit";
        var creditIdempotencyKey = $"{baseIdempotencyKey}-credit";
        var chargebackIdempotencyKey = $"{baseIdempotencyKey}-chargeback";

        try
        {
            await _checkingAccountClient.CreateMovementAsync(
                null,
                amount,
                'D',
                jwtToken,
                debitIdempotencyKey);

            try
            {
                await _checkingAccountClient.CreateMovementAsync(
                    destinationAccountNumber,
                    amount,
                    'C',
                    jwtToken,
                    creditIdempotencyKey);

                return SagaResult.Success();
            }
            catch (Exception creditException)
            {
                try
                {
                    await _checkingAccountClient.CreateMovementAsync(
                        null,
                        amount,
                        'C',
                        jwtToken,
                        chargebackIdempotencyKey);

                    return SagaResult.FailedWithRollback(
                        $"Falha ao creditar conta de destino. Transação revertida. Detalhes: {creditException.Message}");
                }
                catch (Exception)
                {
                    return SagaResult.FailedWithoutRollback(
                        $"Falha crítica: não foi possível reverter a transação. Intervenção manual necessária. Valor: {amount}");
                }
            }
        }
        catch (Exception debitException)
        {
            return SagaResult.FailedAtDebit($"Falha ao debitar conta de origem: {debitException.Message}");
        }
    }
}