using BankingApp.TransferService.Domain.Entities;

namespace BankingApp.TransferService.Domain.Interfaces;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(Guid id);
    Task AddAsync(Transfer transfer);
}