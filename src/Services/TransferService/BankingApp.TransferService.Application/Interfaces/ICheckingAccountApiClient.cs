namespace BankingApp.TransferService.Application.Interfaces;

public interface ICheckingAccountApiClient
{
    Task CreateMovementAsync(
        long? targetAccountNumber,
        decimal amount,
        char movementType,
        string jwtToken,
        string idempotencyKey);
    
    Task<Guid?> GetAccountIdByNumberAsync(long accountNumber, string jwtToken);
}