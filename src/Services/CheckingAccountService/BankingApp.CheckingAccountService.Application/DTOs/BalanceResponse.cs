namespace BankingApp.CheckingAccountService.Application.DTOs;

public record BalanceResponse(
    long AccountNumber,
    string Name,
    DateTime QueryDateTime,
    decimal Balance
);