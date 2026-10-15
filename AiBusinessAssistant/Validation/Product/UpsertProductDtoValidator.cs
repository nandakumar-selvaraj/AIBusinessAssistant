using AiBusinessAssistant.DTOs.Product;
using FluentValidation;

namespace AiBusinessAssistant.Validation.Product;

public sealed class UpsertProductDtoValidator : AbstractValidator<UpsertProductDto>
{
    public UpsertProductDtoValidator()
    {
        RuleFor(product => product.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Product name is required.")
            .MaximumLength(160);
        RuleFor(product => product.SKU)
            .Must(sku => !string.IsNullOrWhiteSpace(sku)).WithMessage("SKU is required.")
            .MaximumLength(64);
        RuleFor(product => product.Description).MaximumLength(1000);
        RuleFor(product => product.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(product => product.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
