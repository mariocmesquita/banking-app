using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Handlers;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCpf_ShouldReturnToken()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();
        var mockJwtService = new Mock<IJwtTokenService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );

        mockRepository.Setup(r => r.GetByCpfAsync("12345678909"))
            .ReturnsAsync(account);
        mockPasswordService.Setup(p => p.VerifyPassword("senha123", "hashedPassword"))
            .Returns(true);
        mockJwtService.Setup(j => j.GenerateToken(It.IsAny<CheckingAccount>()))
            .Returns(("tokenJWT123", DateTime.UtcNow.AddHours(1)));

        var handler = new LoginCommandHandler(mockRepository.Object, mockPasswordService.Object, mockJwtService.Object);
        var command = new LoginCommand("12345678909", "senha123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("tokenJWT123");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ValidAccountNumber_ShouldReturnToken()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();
        var mockJwtService = new Mock<IJwtTokenService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );

        mockRepository.Setup(r => r.GetByAccountNumberAsync(1234567890))
            .ReturnsAsync(account);
        mockPasswordService.Setup(p => p.VerifyPassword("senha123", "hashedPassword"))
            .Returns(true);
        mockJwtService.Setup(j => j.GenerateToken(It.IsAny<CheckingAccount>()))
            .Returns(("tokenJWT123", DateTime.UtcNow.AddHours(1)));

        var handler = new LoginCommandHandler(mockRepository.Object, mockPasswordService.Object, mockJwtService.Object);
        var command = new LoginCommand("1234567890", "senha123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().Be("tokenJWT123");
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowUnauthorized()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();
        var mockJwtService = new Mock<IJwtTokenService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );

        mockRepository.Setup(r => r.GetByCpfAsync("12345678909"))
            .ReturnsAsync(account);
        mockPasswordService.Setup(p => p.VerifyPassword("senhaErrada", "hashedPassword"))
            .Returns(false);

        var handler = new LoginCommandHandler(mockRepository.Object, mockPasswordService.Object, mockJwtService.Object);
        var command = new LoginCommand("12345678909", "senhaErrada");

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Credenciais*");
    }

    [Fact]
    public async Task Handle_InactiveAccount_ShouldThrowException()
    {
        var mockRepository = new Mock<ICheckingAccountRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();
        var mockJwtService = new Mock<IJwtTokenService>();

        var account = CheckingAccount.Create(
            new Cpf("12345678909"),
            "João Silva",
            new AccountNumber(1234567890),
            "hashedPassword",
            "salt"
        );
        account.Deactivate();

        mockRepository.Setup(r => r.GetByCpfAsync("12345678909"))
            .ReturnsAsync(account);

        var handler = new LoginCommandHandler(mockRepository.Object, mockPasswordService.Object, mockJwtService.Object);
        var command = new LoginCommand("12345678909", "senha123");

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InactiveAccountException>()
            .WithMessage("*inativa*");
    }
}
