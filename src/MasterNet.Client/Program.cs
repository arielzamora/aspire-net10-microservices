using MasterNet.Client;
using MasterNet.Client.Configuration;
using MasterNet.Client.Refit;
using MasterNet.Client.Services.Courses;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Options;
using Refit;
using System.Net;
using System.Net.Http.Headers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services
    .AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.WebApiBaseUrl), $"{ApiOptions.SectionName}:WebApiBaseUrl is required.")
    .ValidateOnStart();

builder.Services
    .AddRefitClient<ICoursesApi>()
    .ConfigureHttpClient((sp, client) =>
    {
        var apiOptions = sp.GetRequiredService<IOptions<ApiOptions>>().Value;
        client.BaseAddress = new Uri(apiOptions.WebApiBaseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });

builder.Services.AddScoped<ICoursesService, CoursesService>();

await builder.Build().RunAsync();

static async Task ConfigureAppSettingsAsync(WebAssemblyHostBuilder builder)
{
    using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

    using var appSettingsStream = await httpClient.GetStreamAsync("appsettings.json");
    builder.Configuration.AddJsonStream(appSettingsStream);

    var environmentFile = $"appsettings.{builder.HostEnvironment.Environment}.json";
    try
    {
        using var environmentStream = await httpClient.GetStreamAsync(environmentFile);
        builder.Configuration.AddJsonStream(environmentStream);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        // optional environment file not present
    }
}
