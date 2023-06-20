using Common.Domain.DTOs.ChatDTOs;
using FluentValidation;

namespace Chat.Infrastructure.Validations
{
    public class CreateChatValidator : AbstractValidator<CreateChatDTO>
    {
        public CreateChatValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(30)
                .WithMessage("Chat name must has maximum 30 characters")
                .Matches(@"^[a-zA-Z][a-zA-Z0-9][\p{L} \.\-]+$")
                .WithMessage("Username must start with a letter and not contains special characters");

            RuleFor(p => p.Type)
                .NotEmpty()
                .NotNull()
                .IsInEnum()
                .WithMessage("The type must be one of these: " +
                "1 - Private One to One;" +
                "2 - Public Group;" +
                "3 - Private Group");
        }
    }
}
