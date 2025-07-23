using FluentValidation;

namespace OrderMediatR.Application.Features.Products.CreateProduct
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome do produto é obrigatório")
                .MinimumLength(3).WithMessage("Nome deve ter pelo menos 3 caracteres")
                .MaximumLength(200).WithMessage("Nome não pode exceder 200 caracteres");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Descrição é obrigatória")
                .MinimumLength(10).WithMessage("Descrição deve ter pelo menos 10 caracteres")
                .MaximumLength(1000).WithMessage("Descrição não pode exceder 1000 caracteres");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU é obrigatório")
                .MaximumLength(50).WithMessage("SKU não pode exceder 50 caracteres")
                .Matches(@"^[A-Z0-9\-_]+$").WithMessage("SKU deve conter apenas letras maiúsculas, números, hífen e underscore");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Preço deve ser maior que zero")
                .LessThanOrEqualTo(999999.99m).WithMessage("Preço não pode exceder 999.999,99");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque não pode ser negativa")
                .LessThanOrEqualTo(999999).WithMessage("Quantidade em estoque não pode exceder 999.999");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Categoria é obrigatória")
                .MaximumLength(100).WithMessage("Categoria não pode exceder 100 caracteres");

            RuleFor(x => x.Brand)
                .MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Brand))
                .WithMessage("Marca não pode exceder 100 caracteres");

            RuleFor(x => x.Weight)
                .GreaterThanOrEqualTo(0).WithMessage("Peso não pode ser negativo")
                .LessThanOrEqualTo(999.99m).WithMessage("Peso não pode exceder 999,99 kg");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("URL da imagem não pode exceder 500 caracteres")
                .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("URL da imagem deve ter formato válido");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}