using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public record CreateMovementCommand(
    Guid RequestingAccountId,
    long? TargetAccountNumber,
    decimal Amount,
    char MovementType
) : IRequest<Unit>;