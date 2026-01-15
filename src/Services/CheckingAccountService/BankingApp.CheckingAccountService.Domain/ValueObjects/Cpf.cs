using BankingApp.CheckingAccountService.Domain.Exceptions;

namespace BankingApp.CheckingAccountService.Domain.ValueObjects;

public record Cpf
{
    public Cpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new InvalidCpfException("CPF não pode ser vazio");

        var cleanedCpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cleanedCpf.Length != 11)
            throw new InvalidCpfException("CPF deve conter 11 dígitos");

        if (cleanedCpf.Distinct().Count() == 1)
            throw new InvalidCpfException("CPF inválido");

        if (!ValidateChecksum(cleanedCpf))
            throw new InvalidCpfException("CPF inválido");

        Value = cleanedCpf;
    }

    public string Value { get; }

    private static bool ValidateChecksum(string cpf)
    {
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (cpf[9] - '0' != firstDigit)
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return cpf[10] - '0' == secondDigit;
    }

    public override string ToString()
    {
        return Value;
    }
}