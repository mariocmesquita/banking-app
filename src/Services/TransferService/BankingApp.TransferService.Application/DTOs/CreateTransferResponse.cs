namespace BankingApp.TransferService.Application.DTOs;

public record CreateTransferResponse(
    Guid TransferId,
    DateTime MovementDate,
    string Status
);