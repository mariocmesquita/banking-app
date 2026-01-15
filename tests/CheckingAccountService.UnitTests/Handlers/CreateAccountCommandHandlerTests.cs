using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Handlers;

public class CreateAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidData_ShouldCreateAccount()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();

        mockRepository.Setup(r => r.GetByCpfAsync(It.IsAny<string>()))
            .ReturnsAsync((CheckingAccount?)null);
        mockRepository.Setup(r => r.GetByAccountNumberAsync(It.IsAny<long>()))
            .ReturnsAsync((CheckingAccount?)null);
        mockPasswordService.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns(("hashedPassword", "salt"));

        var handler = new CreateAccountCommandHandler(mockRepository.Object, mockPasswordService.Object);
        var command = new CreateAccountCommand("12345678909", "João Silva", "senha123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.AccountNumber.Should().BeGreaterThan(0);
        mockRepository.Verify(r => r.AddAsync(It.IsAny<CheckingAccount>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCpf_ShouldThrowException()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();

        var existingAccount = CheckingAccount.Create(
            new Domain.ValueObjects.Cpf("12345678909"),
            "Existing User",
            new Domain.ValueObjects.AccountNumber(1234567890),
            "hash",
            "salt"
        );

        mockRepository.Setup(r => r.GetByCpfAsync("12345678909"))
            .ReturnsAsync(existingAccount);

        var handler = new CreateAccountCommandHandler(mockRepository.Object, mockPasswordService.Object);
        var command = new CreateAccountCommand("12345678909", "João Silva", "senha123");

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CPF*cadastrado*");
    }

    [Fact]
    public async Task Handle_InvalidCpf_ShouldThrowException()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();

        var handler = new CreateAccountCommandHandler(mockRepository.Object, mockPasswordService.Object);
        var command = new CreateAccountCommand("00000000000", "João Silva", "senha123");

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCpfException>();
    }
}
