using Chat.Domain.DTOs.FiltersDTO;
using FluentValidation;

namespace Chat.Infrastructure.Validations
{
    public class MessageFilterModelValidator : AbstractValidator<MessageFilterModel>
    {
        public MessageFilterModelValidator()
        {
            RuleFor(p => p.ChatId)
                .NotEmpty()
                .NotNull()
                .WithMessage("Chat identyfire haven't been null or empty");
        }
    }
}
