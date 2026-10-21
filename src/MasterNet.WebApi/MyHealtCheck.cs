using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MasterNet.WebApi
{
    public class MyHealtCheck : IHealthCheck

    {
        public static bool Healthy { get; set; } = true;
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
           context.Registration.Tags.Add("live");

            return Task.FromResult(Healthy ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());     

        }
    }
}
