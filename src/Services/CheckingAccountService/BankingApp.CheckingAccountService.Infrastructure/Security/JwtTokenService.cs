using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BankingApp.CheckingAccountService.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly string _issuer;
    private readonly string _secretKey;

    public JwtTokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
                     ?? throw new InvalidOperationException("Chave JWT não configurada");
        _issuer = configuration["Jwt:Issuer"]
                  ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException("JWT Audience not configured");
        _expirationMinutes = configuration.GetValue("Jwt:ExpirationMinutes", 60);
    }

    public (string token, DateTime expiresAt) GenerateToken(CheckingAccount account)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        var claims = new[]
        {
            new Claim("id_checking_account", account.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresAt);
    }
}