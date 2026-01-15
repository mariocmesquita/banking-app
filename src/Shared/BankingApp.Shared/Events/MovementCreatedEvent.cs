namespace BankingApp.Shared.Events;

public class MovementCreatedEvent
{
    public Guid CheckingAccountId { get; set; }
    public long AccountNumber { get; set; }
    public char MovementType { get; set; }
    public decimal Amount { get; set; }
}
