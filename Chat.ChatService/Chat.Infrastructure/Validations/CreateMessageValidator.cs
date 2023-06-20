using Common.Domain.DTOs.MessageDTOs;
using FluentValidation;

namespace Chat.Infrastructure.Validations
{
    public class CreateMessageValidator : AbstractValidator<CreateMessageDTO>
    {
        public CreateMessageValidator()
        {
            RuleFor(p => p.ChatId)
                .NotEmpty()
                .NotNull();

            RuleFor(p => p.TextMessage)
                .NotEmpty()
                .NotNull()
                .MaximumLength(1024)
                .WithMessage("TextMessage must has maximum 1024 chcharacters");
        }
    }
}
