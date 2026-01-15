using BankingApp.CheckingAccountService.Domain.Entities;

namespace BankingApp.CheckingAccountService.Domain.Interfaces;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) GenerateToken(CheckingAccount account);
}
