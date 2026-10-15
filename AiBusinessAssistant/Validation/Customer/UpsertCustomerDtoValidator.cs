using AiBusinessAssistant.DTOs.Customer;
using FluentValidation;

namespace AiBusinessAssistant.Validation.Customer;

public sealed class UpsertCustomerDtoValidator : AbstractValidator<UpsertCustomerDto>
{
    public UpsertCustomerDtoValidator()
    {
        RuleFor(customer => customer.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Customer name is required.")
            .MaximumLength(120);

        RuleFor(customer => customer.Email)
            .EmailAddress().When(customer => !string.IsNullOrEmpty(customer.Email))
            .MaximumLength(200);

        RuleFor(customer => customer.Phone).MaximumLength(40);
        RuleFor(customer => customer.Address).MaximumLength(500);
    }
}
