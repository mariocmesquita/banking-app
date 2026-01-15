using BankingApp.CheckingAccountService.Application.DTOs;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public record LoginCommand(
    string Identifier,
    string Password
) : IRequest<LoginResponse>;