using FluentValidation;
using PaymentsAPI.Application.Commands;

namespace PaymentsAPI.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId é obrigatório");

        RuleFor(x => x.GameId)
            .NotEmpty()
            .WithMessage("GameId é obrigatório");

        RuleFor(x => x.GameTitle)
            .NotEmpty()
            .WithMessage("GameTitle é obrigatório")
            .MaximumLength(200)
            .WithMessage("GameTitle deve ter no máximo 200 caracteres");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity deve ser maior que zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("UnitPrice deve ser maior que zero");
    }
}
