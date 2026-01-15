using BankingApp.CheckingAccountService.Domain.Entities;

namespace BankingApp.CheckingAccountService.Domain.Interfaces;

public interface ICheckingAccountRepository
{
    Task<CheckingAccount?> GetByIdAsync(Guid id);
    Task<CheckingAccount?> GetByAccountNumberAsync(long accountNumber);
    Task<CheckingAccount?> GetByCpfAsync(string cpf);
    Task AddAsync(CheckingAccount account);
    Task UpdateAsync(CheckingAccount account);
}