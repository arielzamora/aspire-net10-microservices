using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterNet.AppHost.AppHostEvents
{
    public static class ResourceEventsExtensions
    {
        public static void SubscribeToResourcetEvents(this IDistributedApplicationBuilder builder, ProjectResource api)
        {
            builder.Eventing.Subscribe<BeforeResourceStartedEvent>(api,
                static (@event, cancellationToken) =>
                {
                    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("***BeforeResourceStartedEvent*** for {ResourceName}",@event.Resource.Name);
                    return Task.CompletedTask;
                });

            builder.Eventing.Subscribe<ResourceReadyEvent>(api,
                static (@event, cancellationToken) =>
                {
                    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("***ResourceReadyEvent*** for {ResourceName}", @event.Resource.Name);
                    return Task.CompletedTask;
                });

            builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(api,
                static (@event, cancellationToken) =>
                {
                    var logger = @event.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("***ConnectionStringAvailableEvent*** for {ResourceName}", @event.Resource.Name);
                    return Task.CompletedTask;
                }); 
            
        }
    }
}
