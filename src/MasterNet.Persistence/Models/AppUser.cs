using Microsoft.AspNetCore.Identity;

namespace MasterNet.Persistence.Models;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Major { get; set; }
}