using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.WebApi.Extensions;

public static class DataSeed
{
    public static async Task SeedDataAuthentication(
        this IApplicationBuilder app
    )
    {
        using var scope = app.ApplicationServices.CreateScope();
        var service = scope.ServiceProvider;
        var loggerFactory = service.GetRequiredService<ILoggerFactory>();

        try
        {
            var context = service.GetRequiredService<MasterNetDbContext>();
            //await context.Database.MigrateAsync();
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception e)
        {
            var logger = loggerFactory.CreateLogger<MasterNetDbContext>();
            logger.LogError(e.Message);
        }
    }
}