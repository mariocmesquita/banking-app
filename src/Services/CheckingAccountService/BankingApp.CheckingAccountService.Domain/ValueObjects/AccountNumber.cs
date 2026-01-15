namespace BankingApp.CheckingAccountService.Domain.ValueObjects;

public record AccountNumber
{
    public AccountNumber(long value)
    {
        if (value <= 0 || value > 9999999999)
            throw new ArgumentException("Número de conta inválido", nameof(value));

        Value = value;
    }

    public long Value { get; }

    public static AccountNumber GenerateNew()
    {
        var random = Random.Shared.Next(1000000000, 2000000000);
        return new AccountNumber(random);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}