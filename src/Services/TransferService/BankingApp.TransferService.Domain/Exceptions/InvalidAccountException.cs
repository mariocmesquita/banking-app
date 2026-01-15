namespace BankingApp.TransferService.Domain.Exceptions;

public class InvalidAccountException : Exception
{
    public InvalidAccountException(string message) : base(message)
    {
    }
}
