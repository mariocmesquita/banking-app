namespace BankingApp.CheckingAccountService.Application.DTOs;

public record CreateAccountRequest(
    string Cpf,
    string Name,
    string Password
);