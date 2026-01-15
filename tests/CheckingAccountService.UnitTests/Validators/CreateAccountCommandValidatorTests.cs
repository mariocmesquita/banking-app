using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Application.Validators;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.Validators;

public class CreateAccountCommandValidatorTests
{
    [Fact]
    public void Validate_ValidData_ShouldPass()
    {
        var validator = new CreateAccountCommandValidator();
        var command = new CreateAccountCommand("12345678909", "João Silva", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyCpf_ShouldFail()
    {
        var validator = new CreateAccountCommandValidator();
        var command = new CreateAccountCommand("", "João Silva", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cpf");
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var validator = new CreateAccountCommandValidator();
        var command = new CreateAccountCommand("12345678909", "", "senha123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_ShortPassword_ShouldFail()
    {
        var validator = new CreateAccountCommandValidator();
        var command = new CreateAccountCommand("12345678909", "João Silva", "123");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
