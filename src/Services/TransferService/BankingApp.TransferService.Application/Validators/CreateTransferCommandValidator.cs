using BankingApp.TransferService.Application.Commands;
using FluentValidation;

namespace BankingApp.TransferService.Application.Validators;

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.OriginAccountId)
            .NotEmpty()
            .WithMessage("ID da conta de origem é obrigatório");

        RuleFor(x => x.DestinationAccountNumber)
            .GreaterThan(0)
            .WithMessage("Número da conta de destino inválido");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Valor da transferência deve ser maior que zero");

        RuleFor(x => x.JwtToken)
            .NotEmpty()
            .WithMessage("Token de autenticação é obrigatório");
    }
}