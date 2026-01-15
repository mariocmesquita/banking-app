using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Application.Interfaces;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Handlers;

public class CreateMovementCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredit_ShouldSucceed()
    {
        var requestingAccountId = Guid.NewGuid();
        var targetAccountNumber = 1234567890L;
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var targetAccount = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(targetAccountNumber),
            "hashedPassword",
            "salt"
        );

        mockAccountRepository.Setup(r => r.GetByAccountNumberAsync(targetAccountNumber))
            .ReturnsAsync(targetAccount);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(requestingAccountId, targetAccountNumber, 100m, 'C');

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        mockMovementRepository.Verify(r => r.AddAsync(It.IsAny<Movement>()), Times.Once);
        mockKafkaProducer.Verify(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDebit_ShouldSucceed()
    {
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        var accountId = account.Id;

        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(accountId, null, 50m, 'D');

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        mockMovementRepository.Verify(r => r.AddAsync(It.IsAny<Movement>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DebitOnOtherAccount_ShouldThrowException()
    {
        var requestingAccountId = Guid.NewGuid();
        var targetAccountNumber = 1234567890L;
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var targetAccount = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(targetAccountNumber),
            "hashedPassword",
            "salt"
        );

        mockAccountRepository.Setup(r => r.GetByAccountNumberAsync(targetAccountNumber))
            .ReturnsAsync(targetAccount);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(requestingAccountId, targetAccountNumber, 50m, 'D');

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidMovementTypeException>()
            .WithMessage("*crédito*");
    }

    [Fact]
    public async Task Handle_InactiveAccount_ShouldThrowException()
    {
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        account.Deactivate();
        var accountId = account.Id;

        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(accountId, null, 100m, 'C');

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InactiveAccountException>();
    }

    [Fact]
    public async Task Handle_AccountNotFound_ShouldThrowException()
    {
        var accountId = Guid.NewGuid();
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync((CheckingAccount?)null);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(accountId, null, 100m, 'C');

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidAccountException>()
            .WithMessage("Conta não encontrada");
    }

    [Fact]
    public async Task Handle_InvalidValue_ShouldThrowException()
    {
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        var accountId = account.Id;

        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(accountId, null, -50m, 'C');

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidValueException>()
            .WithMessage("Valor deve ser positivo");
    }

    [Fact]
    public async Task Handle_InvalidMovementType_ShouldThrowException()
    {
        var mockAccountRepository = new Mock<ICheckingAccountRepository>();
        var mockMovementRepository = new Mock<IMovementRepository>();
        var mockKafkaProducer = new Mock<IKafkaProducer>();
        var mockCacheService = new Mock<ICacheService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        var accountId = account.Id;

        mockAccountRepository.Setup(r => r.GetByIdAsync(accountId))
            .ReturnsAsync(account);

        var handler = new CreateMovementCommandHandler(
            mockAccountRepository.Object,
            mockMovementRepository.Object,
            mockKafkaProducer.Object,
            mockCacheService.Object
        );
        var command = new CreateMovementCommand(accountId, null, 100m, 'X');

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidMovementTypeException>()
            .WithMessage("*inválido*");
    }
}
