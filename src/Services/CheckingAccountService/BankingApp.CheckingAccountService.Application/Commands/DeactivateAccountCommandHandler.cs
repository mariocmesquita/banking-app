using BankingApp.CheckingAccountService.Domain.Interfaces;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, Unit>
{
    private readonly IPasswordHashingService _passwordService;
    private readonly ICheckingAccountRepository _repository;
    private readonly ICacheService _cacheService;

    public DeactivateAccountCommandHandler(
        ICheckingAccountRepository repository,
        IPasswordHashingService passwordService,
        ICacheService cacheService)
    {
        _repository = repository;
        _passwordService = passwordService;
        _cacheService = cacheService;
    }

    public async Task<Unit> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId);
        if (account == null)
            throw new InvalidOperationException("Conta não encontrada");

        if (!_passwordService.VerifyPassword(request.Password, account.PasswordHash))
            throw new UnauthorizedAccessException("Senha inválida");

        account.Deactivate();

        await _repository.UpdateAsync(account);

        await _cacheService.RemoveAsync($"account:{account.AccountNumber.Value}");
        await _cacheService.RemoveAsync($"balance:{request.AccountId}");

        return Unit.Value;
    }
}