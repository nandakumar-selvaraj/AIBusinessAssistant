using AiBusinessAssistant.DTOs.Chat;
using AiBusinessAssistant.Features.AI.Configuration;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AiBusinessAssistant.Validation.Chat;

public sealed class SendChatRequestDtoValidator : AbstractValidator<SendChatRequestDto>
{
    public SendChatRequestDtoValidator(IOptions<OllamaOptions> options)
    {
        RuleFor(request => request.Message)
            .NotEmpty()
            .Must(message => !string.IsNullOrWhiteSpace(message))
            .WithMessage("Message is required.")
            .MaximumLength(options.Value.MaxMessageLength);

        RuleFor(request => request.ConversationId)
            .GreaterThan(0)
            .When(request => request.ConversationId.HasValue)
            .WithMessage("Conversation ID must be a positive number.");
    }
}
