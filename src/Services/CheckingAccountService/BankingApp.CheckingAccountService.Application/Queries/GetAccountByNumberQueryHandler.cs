using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Queries;

public class GetAccountByNumberQueryHandler : IRequestHandler<GetAccountByNumberQuery, AccountDetailsResponse>
{
    private readonly ICheckingAccountRepository _accountRepository;
    private readonly ICacheService _cacheService;

    public GetAccountByNumberQueryHandler(
        ICheckingAccountRepository accountRepository,
        ICacheService cacheService)
    {
        _accountRepository = accountRepository;
        _cacheService = cacheService;
    }

    public async Task<AccountDetailsResponse> Handle(GetAccountByNumberQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"account:{request.AccountNumber}";

        var cachedAccount = await _cacheService.GetAsync<AccountDetailsResponse>(cacheKey);
        if (cachedAccount != null)
            return cachedAccount;

        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
        if (account == null)
            throw new InvalidOperationException("Conta não encontrada");

        account.EnsureActive();

        var response = new AccountDetailsResponse(
            account.Id,
            account.AccountNumber.Value,
            account.Name,
            account.Active
        );

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

        return response;
    }
}