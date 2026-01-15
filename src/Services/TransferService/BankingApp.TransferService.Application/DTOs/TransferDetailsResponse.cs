namespace BankingApp.TransferService.Application.DTOs;

public record TransferDetailsResponse(
    Guid Id,
    Guid OriginAccountId,
    Guid DestinationAccountId,
    DateTime MovementDate,
    decimal Amount,
    string Status
);