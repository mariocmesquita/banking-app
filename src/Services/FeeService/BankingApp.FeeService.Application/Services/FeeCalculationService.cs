using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace BankingApp.FeeService.Application.Services;

public class FeeCalculationService
{
    private readonly decimal _transferFeeAmount;

    public FeeCalculationService(IConfiguration configuration)
    {
        var feeAmountStr = configuration["FeeConfig:TransferFeeAmount"];

        if (!decimal.TryParse(
                feeAmountStr,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out _transferFeeAmount))
        {
            throw new InvalidOperationException(
                "Configuração de valor de taxa 'FeeConfig:TransferFeeAmount' não está configurada ou está inválida.");
        }

        if (_transferFeeAmount < 0)
        {
            throw new InvalidOperationException(
                "Configuração de valor de taxa 'FeeConfig:TransferFeeAmount' não pode ser negativa.");
        }
    }

    public decimal CalculateTransferFee()
    {
        return _transferFeeAmount;
    }
}