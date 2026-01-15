using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using MediatR;

namespace BankingApp.CheckingAccountService.Application.Queries;

public class GetBalanceQueryHandler : IRequestHandler<GetBalanceQuery, BalanceResponse>
{
    private readonly ICheckingAccountRepository _accountRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly ICacheService _cacheService;

    public GetBalanceQueryHandler(
        ICheckingAccountRepository accountRepository,
        IMovementRepository movementRepository,
        ICacheService cacheService)
    {
        _accountRepository = accountRepository;
        _movementRepository = movementRepository;
        _cacheService = cacheService;
    }

    public async Task<BalanceResponse> Handle(GetBalanceQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"balance:{request.AccountId}";

        var cachedBalance = await _cacheService.GetAsync<BalanceResponse>(cacheKey);
        if (cachedBalance != null)
            return cachedBalance;

        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
            throw new InvalidOperationException("Conta não encontrada");

        account.EnsureActive();

        var balance = await _movementRepository.GetBalanceAsync(request.AccountId);

        var response = new BalanceResponse(
            account.AccountNumber.Value,
            account.Name,
            DateTime.UtcNow,
            balance
        );

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        return response;
    }
}