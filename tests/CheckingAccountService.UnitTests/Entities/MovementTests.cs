using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Enums;
using BankingApp.CheckingAccountService.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Entities;

public class MovementTests
{
    [Fact]
    public void Create_ValidDebitMovement_ShouldSucceed()
    {
        var accountId = Guid.NewGuid();
        var amount = 100.50m;

        var movement = Movement.Create(accountId, MovementType.Debit, amount);

        movement.Should().NotBeNull();
        movement.CheckingAccountId.Should().Be(accountId);
        movement.MovementType.Should().Be(MovementType.Debit);
        movement.Amount.Should().Be(amount);
        movement.MovementDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ValidCreditMovement_ShouldSucceed()
    {
        var accountId = Guid.NewGuid();
        var amount = 500.00m;

        var movement = Movement.Create(accountId, MovementType.Credit, amount);

        movement.Should().NotBeNull();
        movement.CheckingAccountId.Should().Be(accountId);
        movement.MovementType.Should().Be(MovementType.Credit);
        movement.Amount.Should().Be(amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public void Create_NegativeOrZeroAmount_ShouldThrowException(decimal invalidAmount)
    {
        var accountId = Guid.NewGuid();

        Action act = () => Movement.Create(accountId, MovementType.Credit, invalidAmount);

        act.Should().Throw<InvalidValueException>()
            .WithMessage("*positivo*");
    }

    [Fact]
    public void Create_WithValidParameters_ShouldSetMovementId()
    {
        var accountId = Guid.NewGuid();
        var amount = 100m;

        var movement = Movement.Create(accountId, MovementType.Debit, amount);

        movement.Id.Should().NotBe(Guid.Empty);
    }
}
