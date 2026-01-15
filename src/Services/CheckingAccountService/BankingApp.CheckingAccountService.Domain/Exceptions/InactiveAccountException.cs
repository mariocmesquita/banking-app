namespace BankingApp.CheckingAccountService.Domain.Exceptions;

public class InactiveAccountException : Exception
{
    public InactiveAccountException() : base("Conta inativa")
    {
    }
}