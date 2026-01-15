using BankingApp.CheckingAccountService.Application.DTOs;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Queries;

public record GetBalanceQuery(
    Guid AccountId
) : IRequest<BalanceResponse>;