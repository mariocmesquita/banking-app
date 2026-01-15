using BankingApp.FeeService.Domain.Entities;
using FluentAssertions;

namespace BankingApp.FeeService.UnitTests.Entities;

public class FeeTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var checkingAccountId = Guid.NewGuid();
        var amount = 100.50m;

        var fee = Fee.Create(checkingAccountId, amount);

        fee.Should().NotBeNull();
        fee.Id.Should().NotBe(Guid.Empty);
        fee.CheckingAccountId.Should().Be(checkingAccountId);
        fee.Amount.Should().Be(amount);
        fee.MovementDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_EmptyCheckingAccountId_ShouldThrowException()
    {
        var emptyAccountId = Guid.Empty;
        var amount = 100m;

        Action act = () => Fee.Create(emptyAccountId, amount);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*conta corrente inválido*");
    }

    [Fact]
    public void Create_ZeroAmount_ShouldThrowException()
    {
        var checkingAccountId = Guid.NewGuid();
        var zeroAmount = 0m;

        Action act = () => Fee.Create(checkingAccountId, zeroAmount);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void Create_NegativeAmount_ShouldThrowException()
    {
        var checkingAccountId = Guid.NewGuid();
        var negativeAmount = -100m;

        Action act = () => Fee.Create(checkingAccountId, negativeAmount);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*maior que zero*");
    }
}
