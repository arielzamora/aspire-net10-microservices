using MasterNet.Domain.Security;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MasterNet.Persistence;

public static class SeedDatabase
{
    // Contraseña de los usuarios sembrados: va en user-secrets, nunca en el repositorio.
    public const string PasswordConfigKey = "SeedAdmin:Password";

    public static async Task SeedRolesAndUsersAsync(DbContext context, ILogger? logger, CancellationToken cancellationToken)
    {
        try
        {
            var password = context.GetService<IConfiguration>()[PasswordConfigKey];
            await SeedRolesAndUsersAsync(context.GetService<UserManager<AppUser>>(), password, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to seed roles and users");
        }
    }

    public static async Task SeedRolesAndUsersAsync(UserManager<AppUser> userManager, string? password, ILogger? logger, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                logger?.LogWarning("Seed de usuarios omitido: falta la configuración {Key} (user-secrets).", PasswordConfigKey);
                return;
            }

            if (userManager.Users.Any()) return;

            var userAdmin = new AppUser
            {
                FullName = "Ariel Zamora",
                UserName = "azamora",
                Email = "azamora@gmail.com"
            };

            var createAdminRes = await userManager.CreateAsync(userAdmin, password);
            await LogIdentityResultAsync(Task.FromResult(createAdminRes), logger, "Create user azamora");
            if (createAdminRes.Succeeded)
            {
                await LogIdentityResultAsync(userManager.AddToRoleAsync(userAdmin, CustomRoles.ADMIN), logger, "Add user azamora to ADMIN");
            }

            var userClient = new AppUser
            {
                FullName = "John Doe",
                UserName = "johndoe",
                Email = "john.doe@gmail.com"
            };

            var createClientRes = await userManager.CreateAsync(userClient, password);
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