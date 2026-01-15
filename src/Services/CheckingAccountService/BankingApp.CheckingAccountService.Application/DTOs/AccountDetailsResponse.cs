namespace BankingApp.CheckingAccountService.Application.DTOs;

public record AccountDetailsResponse(
    Guid Id,
    long AccountNumber,
    string Name,
    bool Active
);