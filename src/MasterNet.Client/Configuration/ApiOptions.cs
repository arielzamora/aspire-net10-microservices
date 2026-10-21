namespace MasterNet.Client.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "ApiOptions";
    public string WebApiBaseUrl { get; set; } = string.Empty;
}