using BankingApp.CheckingAccountService.Domain.Entities;

namespace BankingApp.CheckingAccountService.Domain.Interfaces;

public interface IMovementRepository
{
    Task AddAsync(Movement movement);
    Task<decimal> GetBalanceAsync(Guid accountId);
}