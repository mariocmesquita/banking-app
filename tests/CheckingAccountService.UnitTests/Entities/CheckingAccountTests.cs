using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Entities;

public class CheckingAccountTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var cpf = new Cpf("12345678909");
        var name = "João Silva";
        var accountNumber = new AccountNumber(1234567890);
        var passwordHash = "hashedPassword123";
        var salt = "salt123";

        var account = CheckingAccount.Create(cpf, name, accountNumber, passwordHash, salt);

        account.Should().NotBeNull();
        account.Id.Should().NotBe(Guid.Empty);
        account.Cpf.Should().Be(cpf);
        account.Name.Should().Be(name);
        account.AccountNumber.Should().Be(accountNumber);
        account.PasswordHash.Should().Be(passwordHash);
        account.Salt.Should().Be(salt);
        account.Active.Should().BeTrue();
        account.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyName_ShouldThrowException()
    {
        var cpf = new Cpf("12345678909");
        var accountNumber = new AccountNumber(1234567890);
        var passwordHash = "hashedPassword123";
        var salt = "salt123";

        Action act = () => CheckingAccount.Create(cpf, "", accountNumber, passwordHash, salt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Fact]
    public void Create_EmptyPasswordHash_ShouldThrowException()
    {
        var cpf = new Cpf("12345678909");
        var name = "João Silva";
        var accountNumber = new AccountNumber(1234567890);
        var salt = "salt123";

        Action act = () => CheckingAccount.Create(cpf, name, accountNumber, "", salt);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash*");
    }

    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        var cpf = new Cpf("12345678909");
        var name = "João Silva";
        var accountNumber = new AccountNumber(1234567890);
        var passwordHash = "hashedPassword123";
        var salt = "salt123";
        var account = CheckingAccount.Create(cpf, name, accountNumber, passwordHash, salt);

        account.Deactivate();

        account.Active.Should().BeFalse();
        account.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EnsureActive_InactiveAccount_ShouldThrowException()
    {
        var cpf = new Cpf("12345678909");
        var name = "João Silva";
        var accountNumber = new AccountNumber(1234567890);
        var passwordHash = "hashedPassword123";
        var salt = "salt123";
        var account = CheckingAccount.Create(cpf, name, accountNumber, passwordHash, salt);
        account.Deactivate();

        Action act = () => account.EnsureActive();

        act.Should().Throw<InactiveAccountException>();
    }
}
