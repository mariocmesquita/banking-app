using BankingApp.FeeService.Domain.Entities;

namespace BankingApp.FeeService.Domain.Interfaces;

public interface IFeeRepository
{
    Task AddAsync(Fee fee);
}