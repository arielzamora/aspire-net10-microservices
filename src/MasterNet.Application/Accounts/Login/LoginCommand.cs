using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Interfaces;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Application.Accounts.Login;

public class LoginCommand
{

    public record LoginCommandRequest(string? Email, string? Password)
    : IRequest<Result<Profile>>, ICommandBase;

    // Make the handler public so the scanner/DI can register it
    public class LoginCommandHandler
    : IRequestHandler<LoginCommandRequest, Result<Profile>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _token_service;

        public LoginCommandHandler(
            UserManager<AppUser> userManager,
            ITokenService tokenService
        )
        {
            _userManager = userManager;
            _token_service = tokenService;
        }

        public async Task<Result<Profile>> Handle(
            LoginCommandRequest request,
            CancellationToken cancellationToken
        )
        {
            var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user is null)
            {
                return Result<Profile>.Failure("User not found");
            }

            var valid = await _userManager
            .CheckPasswordAsync(user, request.Password!);

            if (!valid)
            {
                return Result<Profile>.Failure("Invalid credentials");
            }

            var profile = new Profile
            {
                Email = user.Email,
                FullName = user.FullName,
                Username = user.UserName,
                Token = await _token_service.CreateToken(user)
            };

            return Result<Profile>.Success(profile);
        }
    }

    public sealed class LoginCommandRequestValidator
  : IValidator<LoginCommandRequest>
    {
        public IEnumerable<ValidationError> Validate(LoginCommandRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                yield return new ValidationError("Email", "Email is required");

            if (string.IsNullOrWhiteSpace(req.Password))
                yield return new ValidationError("Password", "Password is required.");
        }
    }

}