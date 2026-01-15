using BankingApp.TransferService.Domain.Entities;
using BankingApp.TransferService.Domain.Enums;
using FluentAssertions;

namespace BankingApp.TransferService.UnitTests.Entities;

public class TransferTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        var originAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var amount = 100.50m;

        var transfer = Transfer.Create(originAccountId, destinationAccountId, amount);

        transfer.Should().NotBeNull();
        transfer.Id.Should().NotBe(Guid.Empty);
        transfer.OriginAccountId.Should().Be(originAccountId);
        transfer.DestinationAccountId.Should().Be(destinationAccountId);
        transfer.Amount.Should().Be(amount);
        transfer.Status.Should().Be(TransferStatus.Pending);
        transfer.MovementDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(-100.50)]
    public void Create_NegativeOrZeroAmount_ShouldThrowException(decimal invalidAmount)
    {
        var originAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        Action act = () => Transfer.Create(originAccountId, destinationAccountId, invalidAmount);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*maior que zero*");
    }

    [Fact]
    public void Create_SameOriginAndDestination_ShouldThrowException()
    {
        var accountId = Guid.NewGuid();

        Action act = () => Transfer.Create(accountId, accountId, 100m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*não podem ser iguais*");
    }

    [Fact]
    public void MarkAsCompleted_ShouldSetStatusCompleted()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        transfer.MarkAsCompleted();

        transfer.Status.Should().Be(TransferStatus.Completed);
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusFailed()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        transfer.MarkAsFailed();

        transfer.Status.Should().Be(TransferStatus.Failed);
    }

    [Fact]
    public void MarkAsRolledBack_FromFailedStatus_ShouldSucceed()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        transfer.MarkAsFailed();

        transfer.MarkAsRolledBack();

        transfer.Status.Should().Be(TransferStatus.RolledBack);
    }

    [Fact]
    public void MarkAsRolledBack_FromPendingStatus_ShouldThrowException()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        Action act = () => transfer.MarkAsRolledBack();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*falha*");
    }
}
