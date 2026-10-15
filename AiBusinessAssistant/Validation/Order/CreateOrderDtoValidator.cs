using AiBusinessAssistant.DTOs.Order;
using FluentValidation;

namespace AiBusinessAssistant.Validation.Order;

public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(order => order.OrderNumber)
            .Must(number => !string.IsNullOrWhiteSpace(number)).WithMessage("Order number is required.")
            .MaximumLength(50);
        RuleFor(order => order.CustomerId).GreaterThan(0);
        RuleFor(order => order.OrderItems).NotNull().NotEmpty();
        RuleForEach(order => order.OrderItems).SetValidator(new CreateOrderItemDtoValidator());
    }
}
