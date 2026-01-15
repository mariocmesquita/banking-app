namespace BankingApp.CheckingAccountService.Application.DTOs;

public record LoginRequest(
    string Identifier,
    string Password
);