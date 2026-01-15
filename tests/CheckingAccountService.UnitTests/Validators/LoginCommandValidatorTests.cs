using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Application.Validators;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Validators;

public class LoginCommandValidatorTests
{
    [Fact]
    public void Validate_ValidData_ShouldPass()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("12345678909", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyIdentifier_ShouldFail()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identifier");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("12345678909", "");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhitespaceIdentifier_ShouldFail()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("   ", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identifier");
    }

    [Fact]
    public void Validate_BothFieldsEmpty_ShouldFail()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("", "");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Identifier");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
