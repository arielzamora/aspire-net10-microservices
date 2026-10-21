using MasterNet.Persistence.Models;

namespace MasterNet.Application.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user);
}