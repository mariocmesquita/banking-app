using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Application.Queries;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Handlers;

public class GetBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_AccountWithMovements_ShouldCalculateBalance()
    {
        var accountId = Guid.NewGuid();
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );

        mockCacheService.Setup(c => c.GetAsync<BalanceResponse>(It.IsAny<string>()))
            .ReturnsAsync((BalanceResponse?)null);
        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        mockMovementRepository.Setup(r => r.GetBalanceAsync(accountId))
            .ReturnsAsync(1500.50m);

        var handler = new GetBalanceQueryHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockCacheService.Object
        );
        var query = new GetBalanceQuery(accountId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Balance.Should().Be(1500.50m);
        result.AccountNumber.Should().Be(1234567890);
        result.Name.Should().Be("João Silva");
    }

    [Fact]
    public async Task Handle_NoMovements_ShouldReturnZero()
    {
        var accountId = Guid.NewGuid();
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );

        mockCacheService.Setup(c => c.GetAsync<BalanceResponse>(It.IsAny<string>()))
            .ReturnsAsync((BalanceResponse?)null);
        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);
        mockMovementRepository.Setup(r => r.GetBalanceAsync(accountId))
            .ReturnsAsync(0m);

        var handler = new GetBalanceQueryHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockCacheService.Object
        );
        var query = new GetBalanceQuery(accountId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_InactiveAccount_ShouldThrowException()
    {
        var accountId = Guid.NewGuid();
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        account.Deactivate();

        mockCacheService.Setup(c => c.GetAsync<BalanceResponse>(It.IsAny<string>()))
            .ReturnsAsync((BalanceResponse?)null);
        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        var handler = new GetBalanceQueryHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockCacheService.Object
        );
        var query = new GetBalanceQuery(accountId);

        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InactiveAccountException>();
    }
}
