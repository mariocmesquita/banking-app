using BankingApp.CheckingAccountService.Domain.Exceptions;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BankingApp.CheckingAccountService.UnitTests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("12345678909")]
    [InlineData("111.444.777-35")]
    [InlineData("52998224725")]
    public void Cpf_ValidCpf_ShouldCreateSuccessfully(string validCpf)
    {
        var cpf = new Cpf(validCpf);

        cpf.Should().NotBeNull();
        cpf.Value.Should().HaveLength(11);
        cpf.Value.Should().MatchRegex("^[0-9]{11}$");
    }

    [Fact]
    public void Cpf_WithFormatting_ShouldRemoveFormatting()
    {
        var formattedCpf = "123.456.789-09";

        var cpf = new Cpf(formattedCpf);

        cpf.Value.Should().Be("12345678909");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cpf_EmptyOrNull_ShouldThrowException(string? invalidCpf)
    {
        Action act = () => new Cpf(invalidCpf!);

        act.Should().Throw<InvalidCpfException>()
            .WithMessage("CPF não pode ser vazio");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("12345")]
    public void Cpf_InvalidLength_ShouldThrowException(string invalidCpf)
    {
        Action act = () => new Cpf(invalidCpf);

        act.Should().Throw<InvalidCpfException>()
            .WithMessage("CPF deve conter 11 dígitos");
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("22222222222")]
    [InlineData("33333333333")]
    [InlineData("99999999999")]
    public void Cpf_AllDigitsEqual_ShouldThrowException(string invalidCpf)
    {
        Action act = () => new Cpf(invalidCpf);

        act.Should().Throw<InvalidCpfException>()
            .WithMessage("CPF inválido");
    }

    [Theory]
    [InlineData("12345678900")]
    [InlineData("11111111112")]
    [InlineData("12345678901")]
    public void Cpf_InvalidChecksum_ShouldThrowException(string invalidCpf)
    {
        Action act = () => new Cpf(invalidCpf);

        act.Should().Throw<InvalidCpfException>()
            .WithMessage("CPF inválido");
    }

    [Fact]
    public void Cpf_ToString_ShouldReturnValue()
    {
        var cpf = new Cpf("12345678909");

        var result = cpf.ToString();

        result.Should().Be("12345678909");
    }

    [Fact]
    public void Cpf_Equality_ShouldWorkCorrectly()
    {
        var cpf1 = new Cpf("12345678909");
        var cpf2 = new Cpf("123.456.789-09");

        cpf1.Should().Be(cpf2);
    }
}
