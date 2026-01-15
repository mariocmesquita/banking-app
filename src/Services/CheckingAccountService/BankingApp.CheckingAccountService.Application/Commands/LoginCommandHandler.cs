using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashingService _passwordService;
    private readonly ICheckingAccountRepository _repository;

    public LoginCommandHandler(
        ICheckingAccountRepository repository,
        IPasswordHashingService passwordService,
        IJwtTokenService jwtService)
    {
        _repository = repository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        CheckingAccount? account;

        var cleanIdentifier = new string(request.Identifier.Where(char.IsDigit).ToArray());
        if (cleanIdentifier.Length == 11)
        {
            account = await _repository.GetByCpfAsync(cleanIdentifier);
        }
        else if (cleanIdentifier.Length > 0 && cleanIdentifier.Length <= 10 && long.TryParse(cleanIdentifier, out var accountNumber))
        {
            account = await _repository.GetByAccountNumberAsync(accountNumber);
        }
        else
        {
            throw new ArgumentException("Identificador inválido. Use CPF (11 dígitos) ou número da conta (até 10 dígitos)");
        }

        if (account == null)
            throw new UnauthorizedAccessException("Credenciais inválidas");

        account.EnsureActive();

        if (!_passwordService.VerifyPassword(request.Password, account.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas");

        var (token, expiresAt) = _jwtService.GenerateToken(account);

        return new LoginResponse(token, expiresAt);
    }
}