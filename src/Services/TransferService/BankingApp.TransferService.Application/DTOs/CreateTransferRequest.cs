namespace BankingApp.TransferService.Application.DTOs;

public record CreateTransferRequest(
    long DestinationAccountNumber,
    decimal Amount
);