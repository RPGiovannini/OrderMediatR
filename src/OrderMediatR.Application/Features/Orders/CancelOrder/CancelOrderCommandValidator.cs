using FluentValidation;

namespace OrderMediatR.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID do pedido é obrigatório");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Motivo do cancelamento é obrigatório")
                .MinimumLength(10).WithMessage("Motivo deve ter pelo menos 10 caracteres")
                .MaximumLength(500).WithMessage("Motivo não pode exceder 500 caracteres");
        }
    }
} 