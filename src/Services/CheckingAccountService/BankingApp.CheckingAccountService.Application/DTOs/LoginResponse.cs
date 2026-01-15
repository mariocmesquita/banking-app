namespace BankingApp.CheckingAccountService.Application.DTOs;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt
);