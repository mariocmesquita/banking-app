using BankingApp.CheckingAccountService.Application.DTOs;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Queries;

public record GetAccountByNumberQuery(
    long AccountNumber
) : IRequest<AccountDetailsResponse>;