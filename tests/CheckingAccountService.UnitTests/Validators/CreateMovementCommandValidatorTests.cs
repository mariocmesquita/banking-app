using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Application.Validators;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Validators;

public class CreateMovementCommandValidatorTests
{
    [Fact]
    public void Validate_ValidData_ShouldPass()
    {
        var validator = new CreateMovementCommandValidator();
        var command = new CreateMovementCommand(Guid.NewGuid(), 1234567890, 100m, 'C');

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ZeroAmount_ShouldFail()
    {
        var validator = new CreateMovementCommandValidator();
        var command = new CreateMovementCommand(Guid.NewGuid(), 1234567890, 0m, 'C');

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_NegativeAmount_ShouldFail()
    {
        var validator = new CreateMovementCommandValidator();
        var command = new CreateMovementCommand(Guid.NewGuid(), 1234567890, -50m, 'C');

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Validate_InvalidMovementType_ShouldFail()
    {
        var validator = new CreateMovementCommandValidator();
        var command = new CreateMovementCommand(Guid.NewGuid(), 1234567890, 100m, 'X');

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovementType");
    }

    [Fact]
    public void Validate_InvalidMovementTypeNumeric_ShouldFail()
    {
        var validator = new CreateMovementCommandValidator();
        var command = new CreateMovementCommand(Guid.NewGuid(), 1234567890, 100m, '1');

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MovementType");
    }
}
