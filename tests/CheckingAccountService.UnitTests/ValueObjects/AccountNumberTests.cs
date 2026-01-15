using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.ValueObjects;

public class AccountNumberTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(1000000000)]
    [InlineData(5000000000)]
    [InlineData(9999999999)]
    public void Constructor_ValidNumber_ShouldSucceed(long validNumber)
    {
        var accountNumber = new AccountNumber(validNumber);

        accountNumber.Should().NotBeNull();
        accountNumber.Value.Should().Be(validNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-1000)]
    public void Constructor_ZeroOrNegative_ShouldThrowException(long invalidNumber)
    {
        Action act = () => new AccountNumber(invalidNumber);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*inválido*");
    }

    [Theory]
    [InlineData(10000000000)]
    [InlineData(99999999999)]
    [InlineData(long.MaxValue)]
    public void Constructor_AboveMax_ShouldThrowException(long invalidNumber)
    {
        Action act = () => new AccountNumber(invalidNumber);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*inválido*");
    }

    [Fact]
    public void GenerateNew_ShouldReturnValidNumber()
    {
        var accountNumber = AccountNumber.GenerateNew();

        accountNumber.Should().NotBeNull();
        accountNumber.Value.Should().BeGreaterThan(0);
        accountNumber.Value.Should().BeLessThanOrEqualTo(9999999999);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var accountNumber = new AccountNumber(1234567890);

        var result = accountNumber.ToString();

        result.Should().Be("1234567890");
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var accountNumber1 = new AccountNumber(1234567890);
        var accountNumber2 = new AccountNumber(1234567890);

        accountNumber1.Should().Be(accountNumber2);
    }
}
