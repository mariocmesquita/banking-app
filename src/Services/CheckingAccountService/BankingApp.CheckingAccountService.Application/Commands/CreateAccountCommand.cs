using BankingApp.CheckingAccountService.Application.DTOs;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public record CreateAccountCommand(
    string Cpf,
    string Name,
    string Password
) : IRequest<CreateAccountResponse>;