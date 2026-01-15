namespace BankingApp.CheckingAccountService.Domain.Exceptions;

public class InvalidMovementTypeException : Exception
{
    public InvalidMovementTypeException(string message) : base(message)
    {
    }
}
