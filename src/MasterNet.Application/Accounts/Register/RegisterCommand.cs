using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Interfaces;
using MasterNet.Domain.Security;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MasterNet.Application.Accounts.Login.LoginCommand;

namespace MasterNet.Application.Accounts.Register;

public class RegisterCommand
{

    public record RegisterCommandRequest(RegisterRequest registerRequest)
    : IRequest<Result<Profile>>, ICommandBase;


    internal class RegisterCommandHandler
    : IRequestHandler<RegisterCommandRequest, Result<Profile>>
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _token_service;

        public RegisterCommandHandler(
            UserManager<AppUser> userManager,
            ITokenService tokenService
        )
        {
            _userManager = userManager;
            _token_service = tokenService;
        }

        public async Task<Result<Profile>> Handle(RegisterCommandRequest request, CancellationToken cancellationToken)
        {

            if (await _userManager.Users
            .AnyAsync(x => x.Email == request.registerRequest.Email))
            {
                return Result<Profile>.Failure("Email is already registered");
            }

            if (await _userManager.Users
            .AnyAsync(x => x.UserName == request.registerRequest.Username))
            {
                return Result<Profile>.Failure("Username is already registered");
            }

            var user = new AppUser
            {
                FullName = request.registerRequest.FullName,
                Id = Guid.NewGuid().ToString(),
                Major = request.registerRequest.Major,
                Email = request.registerRequest.Email,
                UserName = request.registerRequest.Username
            };

            var result = await _userManager
            .CreateAsync(user, request.registerRequest.Password!);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, CustomRoles.CLIENT);

                var profile = new Profile
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Token = await _token_service.CreateToken(user),
                    Username = user.UserName
                };

                return Result<Profile>.Success(profile);
            }

            return Result<Profile>.Failure("Errors registering user");
        }
    }

    public sealed class RegisterCommandRequestValidator
: IValidator<RegisterCommandRequest>
    {
        public IEnumerable<ValidationError> Validate(RegisterCommandRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.registerRequest.FullName))
                yield return new ValidationError("FullName", "Fullname is required");

            if (string.IsNullOrWhiteSpace(req.registerRequest.Username))
                yield return new ValidationError("Username", "Username is required");

            if (string.IsNullOrWhiteSpace(req.registerRequest.Email))
                yield return new ValidationError("Email", "Email is required");

            if (string.IsNullOrWhiteSpace(req.registerRequest.Password))
                yield return new ValidationError("Password", "Password is required");
        }
    }
}