namespace BankingApp.CheckingAccountService.Domain.Interfaces;

public interface IPasswordHashingService
{
    (string hash, string salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
