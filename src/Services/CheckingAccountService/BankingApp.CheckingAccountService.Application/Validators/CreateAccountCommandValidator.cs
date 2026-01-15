using BankingApp.CheckingAccountService.Application.Commands;
using FluentValidation;

namespace BankingApp.CheckingAccountService.Application.Validators;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11, 14).WithMessage("CPF deve ter 11 dígitos (apenas números) ou 14 caracteres (formatado)");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres")
            .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres");
    }
}