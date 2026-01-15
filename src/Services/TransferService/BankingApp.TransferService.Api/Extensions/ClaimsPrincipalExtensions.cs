using System.Security.Claims;

namespace BankingApp.TransferService.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAccountId(this ClaimsPrincipal principal)
    {
        var accountIdClaim = principal.FindFirst("id_checking_account")?.Value;
        if (string.IsNullOrEmpty(accountIdClaim))
            throw new UnauthorizedAccessException("Token inválido: ID da conta não encontrado");

        return Guid.Parse(accountIdClaim);
    }
}