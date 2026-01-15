using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CreateAccountResponse>
{
    private readonly IPasswordHashingService _passwordService;
    private readonly ICheckingAccountRepository _repository;

    public CreateAccountCommandHandler(
        ICheckingAccountRepository repository,
        IPasswordHashingService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<CreateAccountResponse> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var cpf = new Cpf(request.Cpf);
        var existingAccount = await _repository.GetByCpfAsync(cpf.Value);
        if (existingAccount != null)
            throw new InvalidOperationException("CPF já cadastrado");

        var accountNumber = AccountNumber.GenerateNew();
        var existingByNumber = await _repository.GetByAccountNumberAsync(accountNumber.Value);
        while (existingByNumber != null)
        {
            accountNumber = AccountNumber.GenerateNew();
            existingByNumber = await _repository.GetByAccountNumberAsync(accountNumber.Value);
        }

        var (hash, salt) = _passwordService.HashPassword(request.Password);
        var account = CheckingAccount.Create(cpf, request.Name, accountNumber, hash, salt);

        await _repository.AddAsync(account);

        return new CreateAccountResponse(accountNumber.Value);
    }
}