namespace BankingApp.Shared.Events;

public class FeeAppliedEvent
{
    public Guid FeeId { get; set; }
    public Guid CheckingAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime MovementDate { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
