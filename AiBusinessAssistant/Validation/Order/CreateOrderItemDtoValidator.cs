using AiBusinessAssistant.DTOs.Order;
using FluentValidation;

namespace AiBusinessAssistant.Validation.Order;

public sealed class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
{
    public CreateOrderItemDtoValidator()
    {
        RuleFor(item => item.ProductId).GreaterThan(0);
        RuleFor(item => item.Quantity).GreaterThan(0);
    }
}
