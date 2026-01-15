namespace BankingApp.CheckingAccountService.Domain.Exceptions;

public class InvalidCpfException : Exception
{
    public InvalidCpfException(string message) : base(message)
    {
    }
}