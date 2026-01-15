using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.ValueObjects;

namespace BankingApp.CheckingAccountService.Domain.Entities;

public class CheckingAccount
{
    private CheckingAccount() {}

    private CheckingAccount(
        Cpf cpf,
        string name,
        AccountNumber accountNumber,
        string passwordHash,
        string salt)
    {
        Id = Guid.NewGuid();
        Cpf = cpf;
        Name = name;
        AccountNumber = accountNumber;
        PasswordHash = passwordHash;
        Salt = salt;
        Active = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; }
    public Cpf Cpf { get; private set; }
    public string Name { get; private set; }
    public bool Active { get; private set; }
    public string PasswordHash { get; private set; }
    public string Salt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static CheckingAccount Create(
        Cpf cpf,
        string name,
        AccountNumber accountNumber,
        string passwordHash,
        string salt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório", nameof(name));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash é obrigatório", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(salt))
            throw new ArgumentException("Salt é obrigatório", nameof(salt));

        return new CheckingAccount(cpf, name, accountNumber, passwordHash, salt);
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnsureActive()
    {
        if (!Active)
            throw new InactiveAccountException();
    }
}