using BankingApp.TransferService.Domain.Entities;
using FluentAssertions;

namespace BankingApp.TransferService.UnitTests.Handlers;

public class CreateTransferCommandHandlerTests
{
    [Fact]
    public void Transfer_Create_ValidData_ShouldCreatePendingTransfer()
    {
        var originAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var amount = 100m;

        var transfer = Transfer.Create(originAccountId, destinationAccountId, amount);

        transfer.Should().NotBeNull();
        transfer.OriginAccountId.Should().Be(originAccountId);
        transfer.DestinationAccountId.Should().Be(destinationAccountId);
        transfer.Amount.Should().Be(amount);
    }

    [Fact]
    public void Transfer_MarkAsCompleted_ShouldChangeStatus()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        transfer.MarkAsCompleted();

        transfer.Status.Should().Be(BankingApp.TransferService.Domain.Enums.TransferStatus.Completed);
    }

    [Fact]
    public void Transfer_WithRollback_ShouldMarkAsFailed()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);

        transfer.MarkAsFailed();
        transfer.MarkAsRolledBack();

        transfer.Status.Should().Be(BankingApp.TransferService.Domain.Enums.TransferStatus.RolledBack);
    }
}
