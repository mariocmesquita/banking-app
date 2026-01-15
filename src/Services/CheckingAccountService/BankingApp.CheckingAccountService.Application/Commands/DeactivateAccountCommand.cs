using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public record DeactivateAccountCommand(
    Guid AccountId,
    string Password
) : IRequest<Unit>;