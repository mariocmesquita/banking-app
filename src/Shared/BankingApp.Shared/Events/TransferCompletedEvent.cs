namespace BankingApp.Shared.Events;

public class TransferCompletedEvent
{
    public Guid TransferId { get; set; }
    public Guid OriginAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime MovementDate { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
