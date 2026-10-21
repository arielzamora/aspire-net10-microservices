using MasterNet.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MasterNet.Infrastructure.Security;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUsername()
    {
        return _httpContextAccessor
        .HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;
    }

    public string GetEmail()
    {
        return _httpContextAccessor
        .HttpContext!.User.FindFirstValue(ClaimTypes.Email)!;
    }

}