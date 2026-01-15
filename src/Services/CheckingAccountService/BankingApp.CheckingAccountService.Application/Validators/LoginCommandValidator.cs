using BankingApp.CheckingAccountService.Application.Commands;
using FluentValidation;

namespace BankingApp.CheckingAccountService.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identificador (CPF ou número da conta) é obrigatório");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória");
    }
}