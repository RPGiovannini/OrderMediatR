using FluentValidation;

namespace OrderMediatR.Application.Features.Orders.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Cliente é obrigatório");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Pedido deve ter pelo menos um item")
                .Must(items => items.Count <= 50).WithMessage("Pedido não pode ter mais de 50 itens");

            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());

            RuleFor(x => x.ShippingAmount)
                .GreaterThanOrEqualTo(0).When(x => x.ShippingAmount.HasValue)
                .WithMessage("Valor do frete não pode ser negativo");

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).When(x => x.DiscountAmount.HasValue)
                .WithMessage("Valor do desconto não pode ser negativo");

            RuleFor(x => x.Notes)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes))
                .WithMessage("Observações não podem exceder 500 caracteres");
        }
    }

    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Produto é obrigatório");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero")
                .LessThanOrEqualTo(100).WithMessage("Quantidade não pode exceder 100 unidades");
        }
    }
}