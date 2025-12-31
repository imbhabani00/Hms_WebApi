using FluentValidation;
using Hms.WebApi.Models;

namespace Hms.WebApi.Validators.Users
{

    public class UserAuthValidator : AbstractValidator<AuthModel>
    {
        public UserAuthValidator()
        {
            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Valid email is required.");

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("Valid tenant ID is required.");
        }
    }
}