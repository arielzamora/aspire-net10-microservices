using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterNet.AppHost.AppHostEvents
{
    public static class AppHostEventsExtensions
    {
        public static void SubscribeToAppHostEvents(this IDistributedApplicationBuilder builder)
        {
            builder.Eventing.Subscribe<BeforeStartEvent>(
                static (@event, cancellationToken) =>
                {
                    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("***BeforeStarEvent***");
                    return Task.CompletedTask;
                });

            builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>(
            static (@event, cancellationToken) =>
            {
                var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("***ResourceEndpointsAllocatedEvent***");
                return Task.CompletedTask;
            });

            builder.Eventing.Subscribe<AfterResourcesCreatedEvent>(
            static (@event, cancellationToken) =>
            {
                var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("***AfterResourcesCreatedEvent***");
                return Task.CompletedTask;
            });
        }
    }
}
