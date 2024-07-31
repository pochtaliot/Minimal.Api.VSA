using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Database;
using Minimal.Api.Authentication.Database.Entities;
using Minimal.Api.Shared;
using Minimal.Api.Shared.Extensions;
using Minimal.Api.Shared.Settings;

namespace Minimal.Api.Authentication.Features;

public record CreateUserCommand(string Email, string Password, IEnumerable<CreateUserContactInfoCommand> ContactInfo) : IRequest<Result<CreateUserResponse>>;
public record CreateUserContactInfoCommand(string MobilePhone);
public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<CreateUserResponse>>
{
    private readonly UsersDbContext _dbContext;
    private readonly JwtParameters _jwtParameters;
    public CreateUserHandler(UsersDbContext dbContext, JwtParameters jwtParameters) => 
        (_dbContext, _jwtParameters) = (dbContext, jwtParameters);

    public async Task<Result<CreateUserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = request.Adapt<User>();
        user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
        user.RefreshToken = new UserRefreshToken { Token = StringExtension.GenerateRandomString(_jwtParameters.RefreshTokenLength) };
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(user.Adapt<CreateUserResponse>());
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(UsersDbContext dbContext)
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .EmailAddress()
            .MustAsync(async (email, cancellationToken) => !await dbContext.Users.AnyAsync(a => a.Email == email, cancellationToken))
                .WithMessage("Specified e-mail has already been registered");

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(10)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

        RuleForEach(c => c.ContactInfo)
            .SetValidator(new CreateUserContactInfoCommandValidator());
    }
}

public class CreateUserContactInfoCommandValidator : AbstractValidator<CreateUserContactInfoCommand>
{
    public CreateUserContactInfoCommandValidator()
    {
        RuleFor(c => c.MobilePhone)
            .MaximumLength(20)
            .Matches(@"^\d+$")
                .WithMessage("'MobilePhone' should contain only digits with country code without any special chars")
            .When(x => !string.IsNullOrWhiteSpace(x.MobilePhone));
    }
}