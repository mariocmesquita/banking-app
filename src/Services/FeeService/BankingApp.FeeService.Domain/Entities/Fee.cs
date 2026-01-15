namespace BankingApp.FeeService.Domain.Entities;

public class Fee
{
    private Fee() { }

    public Guid Id { get; private set; }
    public Guid CheckingAccountId { get; private set; }
    public DateTime MovementDate { get; private set; }
    public decimal Amount { get; private set; }

    public static Fee Create(
        Guid checkingAccountId,
        decimal amount)
    {
        if (checkingAccountId == Guid.Empty)
            throw new ArgumentException("ID da conta corrente inválido", nameof(checkingAccountId));

        if (amount <= 0)
            throw new ArgumentException("Valor da tarifa deve ser maior que zero", nameof(amount));

        return new Fee
        {
            Id = Guid.NewGuid(),
            CheckingAccountId = checkingAccountId,
            MovementDate = DateTime.UtcNow,
            Amount = amount
        };
    }
}