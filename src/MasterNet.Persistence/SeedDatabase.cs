using MasterNet.Domain.Security;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

namespace MasterNet.Persistence;

public static class SeedDatabase
{
    public static async Task SeedRolesAndUsersAsync(DbContext context, ILogger? logger, CancellationToken cancellationToken)
    {
        try
        {
            var userManager = context.GetService<UserManager<AppUser>>();

            if (userManager.Users.Any()) return;

            var userAdmin = new AppUser
            {
                FullName = "Vaxi Drez",
                UserName = "vaxidrez",
                Email = "vaxi.drez@gmail.com"
            };

            var createAdminRes = await userManager.CreateAsync(userAdmin, "Password123$");
            await LogIdentityResultAsync(Task.FromResult(createAdminRes), logger, "Create user vaxidrez");
            if (createAdminRes.Succeeded)
            {
                await LogIdentityResultAsync(userManager.AddToRoleAsync(userAdmin, CustomRoles.ADMIN), logger, "Add user vaxidrez to ADMIN");
            }

            var userClient = new AppUser
            {
                FullName = "John Doe",
                UserName = "johndoe",
                Email = "john.doe@gmail.com"
            };

            var createClientRes = await userManager.CreateAsync(userClient, "Password123$");
            await LogIdentityResultAsync(Task.FromResult(createClientRes), logger, "Create user johndoe");
            if (createClientRes.Succeeded)
            {
                await LogIdentityResultAsync(userManager.AddToRoleAsync(userClient, CustomRoles.CLIENT), logger, "Add user johndoe to CLIENT");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to seed roles and users");
        }

        static async Task LogIdentityResultAsync(Task<IdentityResult> task, ILogger? logger, string op)
        {
            try
            {
                var result = await task;
                if (result is null) return;
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    logger?.LogWarning("{Operation} failed: {Errors}", op, errors);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Identity operation failed: {Op}", op);
            }
        }
    }
}