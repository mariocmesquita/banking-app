using BankingApp.TransferService.Domain.Enums;

namespace BankingApp.TransferService.Domain.Entities;

public class Transfer
{
    private Transfer() { }

    public Guid Id { get; private set; }
    public Guid OriginAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public DateTime MovementDate { get; private set; }
    public decimal Amount { get; private set; }
    public TransferStatus Status { get; private set; }

    public static Transfer Create(
        Guid originAccountId,
        Guid destinationAccountId,
        decimal amount)
    {
        if (originAccountId == Guid.Empty)
            throw new ArgumentException("ID da conta de origem inválido", nameof(originAccountId));

        if (destinationAccountId == Guid.Empty)
            throw new ArgumentException("ID da conta de destino inválido", nameof(destinationAccountId));

        if (originAccountId == destinationAccountId)
            throw new ArgumentException("Conta de origem e destino não podem ser iguais");

        if (amount <= 0)
            throw new ArgumentException("Valor da transferência deve ser maior que zero", nameof(amount));

        return new Transfer
        {
            Id = Guid.NewGuid(),
            OriginAccountId = originAccountId,
            DestinationAccountId = destinationAccountId,
            MovementDate = DateTime.UtcNow,
            Amount = amount,
            Status = TransferStatus.Pending
        };
    }

    public void MarkAsCompleted()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException("Apenas transferências pendentes podem ser completadas");

        Status = TransferStatus.Completed;
    }

    public void MarkAsFailed()
    {
        if (Status != TransferStatus.Pending)
            throw new InvalidOperationException("Apenas transferências pendentes podem ser marcadas como falha");

        Status = TransferStatus.Failed;
    }

    public void MarkAsRolledBack()
    {
        if (Status != TransferStatus.Failed)
            throw new InvalidOperationException("Apenas transferências com falha podem ser revertidas");

        Status = TransferStatus.RolledBack;
    }
}