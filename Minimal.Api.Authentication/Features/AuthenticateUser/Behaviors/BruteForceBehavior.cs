using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimal.Api.Authentication.Contracts;
using Minimal.Api.Authentication.Database;
using Minimal.Api.Authentication.Database.Entities;
using Minimal.Api.Authentication.Extensions;
using Minimal.Api.Shared;

namespace Minimal.Api.Authentication.Features.AuthenticateUser.Behaviors;

public class BruteForceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly UsersDbContext _dbContext;
    private readonly ILogger<BruteForceBehavior<TRequest, TResponse>> _logger;
    private readonly IConfiguration _configuration;
    public BruteForceBehavior(ILogger<BruteForceBehavior<TRequest, TResponse>> logger, UsersDbContext dbContext, IConfiguration configuration) =>
        (_logger, _dbContext, _configuration) = (logger, dbContext, configuration);

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is AuthenticateUserCommand authenticateUserCommand)
            return await PipeAuthenticateUserCommand(next, authenticateUserCommand);

        if (request is RefreshTokenCommand refreshTokenCommand)
            return await PipeRefreshTokenCommand(next, refreshTokenCommand);

        return await next();
    }

    private async Task<TResponse> PipeAuthenticateUserCommand(RequestHandlerDelegate<TResponse> next, AuthenticateUserCommand command)
    {
        var loginAttempt = await GetLoginAttempt(command);

        if (loginAttempt is not null && loginAttempt.AttemptsCount > _configuration.GetLoginAttemptsLimit())
        {
            _logger.LogWarning("{0} - {1}, {2}", ApiErrors.LoginAttemptsExceeded, command.ClientIp, command.RemoteHost);
            return (TResponse)(object)Result.Fail<AuthenticateUserResponse>(ApiErrors.LoginAttemptsExceeded);
        }

        var tresult = await next();
        var result = tresult as Result;

        if (result!.Errors.Contains(ApiErrors.LoginFailed))
        {
            if (loginAttempt is null)
                await CreateLoginAttempt(command);
            else
                await RegisterNextLoginAttempt(loginAttempt);
        }

        else if (result.IsSuccess && loginAttempt is not null && loginAttempt.AttemptsCount > 0)
            await ResetLoginAttempts(loginAttempt);

        return tresult;
    }

    private async Task<TResponse> PipeRefreshTokenCommand(RequestHandlerDelegate<TResponse> next, RefreshTokenCommand command)
    {
        var tresult = await next();
        var result = tresult as Result;

        if (!result!.IsSuccess)
        {
            _logger.LogWarning("{0} - {1}, {2}", String.Join("\n", result.Errors), command.ClientIp, command.RemoteHost);
            return (TResponse)(object)Result.Fail<RefreshTokenResponse>(ApiErrors.InvalidToken);
        }

        return tresult;
    }

    private async Task<UserLoginAttempt?> GetLoginAttempt(AuthenticateUserCommand request) =>
        await _dbContext.UsersLoginAttempts.FirstOrDefaultAsync(f => f.Username == request.Username && f.IpAddress == request.ClientIp);

    private async Task CreateLoginAttempt(AuthenticateUserCommand request)
    {
        _dbContext.UsersLoginAttempts.Add(new UserLoginAttempt { Username = request.Username, IpAddress = request.ClientIp });
        await _dbContext.SaveChangesAsync();
    }

    private async Task RegisterNextLoginAttempt(UserLoginAttempt attempt) =>
        await _dbContext.UsersLoginAttempts.Where(w => w.Username == attempt.Username && w.IpAddress == attempt.IpAddress)
                                           .ExecuteUpdateAsync(e => e.SetProperty(p => p.AttemptsCount, attempt!.AttemptsCount + 1));
    private async Task ResetLoginAttempts(UserLoginAttempt attempt) =>
        await _dbContext.UsersLoginAttempts.Where(w => w.Username == attempt.Username && w.IpAddress == attempt.IpAddress)
                                           .ExecuteUpdateAsync(e => e.SetProperty(p => p.AttemptsCount, 0));
}