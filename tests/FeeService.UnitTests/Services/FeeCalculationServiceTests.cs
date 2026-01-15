using BankingApp.FeeService.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BankingApp.FeeService.UnitTests.Services;

public class FeeCalculationServiceTests
{
    [Fact]
    public void CalculateTransferFee_WithConfiguredFee_ShouldReturnConfiguredAmount()
    {
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["FeeConfig:TransferFeeAmount"])
            .Returns("3.50");
        var service = new FeeCalculationService(mockConfiguration.Object);
        var fee = service.CalculateTransferFee();

        fee.Should().Be(3.50m);
    }
}
