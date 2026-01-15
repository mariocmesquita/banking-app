namespace BankingApp.TransferService.Domain.Exceptions;

public class TransferFailedException : Exception
{
    public TransferFailedException(string message) : base(message)
    {
    }
}
