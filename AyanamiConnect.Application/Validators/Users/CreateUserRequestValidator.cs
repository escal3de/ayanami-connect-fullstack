using AyanamiConnect.Application.Contracts.Users;
using FluentValidation;

namespace AyanamiConnect.Application.Validators.Users;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .NotEmpty();

        RuleFor(x => x.UserName)
            .MaximumLength(32);
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(128);
        
        RuleFor(x => x.LastName)
            .MaximumLength(128);
    }
}