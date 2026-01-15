namespace BankingApp.TransferService.Domain.Enums;

public enum TransferStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    RolledBack = 3
}