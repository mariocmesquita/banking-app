using BankingApp.CheckingAccountService.Domain.Interfaces;

namespace BankingApp.CheckingAccountService.Infrastructure.Security;

public class PasswordHashingService : IPasswordHashingService
{
    private const int WorkFactor = 12;

    public (string hash, string salt) HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}