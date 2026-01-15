using BankingApp.CheckingAccountService.Application.Commands;
using FluentValidation;

namespace BankingApp.CheckingAccountService.Application.Validators;

public class CreateMovementCommandValidator : AbstractValidator<CreateMovementCommand>
{
    public CreateMovementCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.MovementType)
            .Must(t => t == 'C' || t == 'D')
            .WithMessage("Tipo de movimento deve ser 'C' (crédito) ou 'D' (débito)");
    }
}