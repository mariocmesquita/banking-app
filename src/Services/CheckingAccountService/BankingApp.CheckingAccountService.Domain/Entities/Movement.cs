using BankingApp.CheckingAccountService.Domain.Enums;
using BankingApp.CheckingAccountService.Domain.Exceptions;

namespace BankingApp.CheckingAccountService.Domain.Entities;

public class Movement
{
    private Movement() { }

    private Movement(Guid checkingAccountId, MovementType movementType, decimal amount)
    {
        Id = Guid.NewGuid();
        CheckingAccountId = checkingAccountId;
        MovementDate = DateTime.UtcNow;
        MovementType = movementType;
        Amount = amount;
    }

    public Guid Id { get; private set; }
    public Guid CheckingAccountId { get; private set; }
    public DateTime MovementDate { get; private set; }
    public MovementType MovementType { get; private set; }
    public decimal Amount { get; private set; }

    public static Movement Create(Guid checkingAccountId, MovementType movementType, decimal amount)
    {
        if (amount <= 0)
            throw new InvalidValueException("Valor deve ser positivo");

        if (!Enum.IsDefined(typeof(MovementType), movementType))
            throw new InvalidMovementTypeException("Tipo de movimento inválido");

        return new Movement(checkingAccountId, movementType, amount);
    }
}