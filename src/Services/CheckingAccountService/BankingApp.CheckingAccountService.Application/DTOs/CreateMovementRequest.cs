namespace BankingApp.CheckingAccountService.Application.DTOs;

public record CreateMovementRequest(
    long? AccountNumber,
    decimal Amount,
    char MovementType
);