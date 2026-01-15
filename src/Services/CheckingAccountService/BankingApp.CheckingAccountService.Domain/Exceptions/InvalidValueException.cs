namespace BankingApp.CheckingAccountService.Domain.Exceptions;

public class InvalidValueException : Exception
{
    public InvalidValueException(string message) : base(message)
    {
    }
}
