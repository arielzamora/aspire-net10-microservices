using System.Net;
using System.Text.Json;

namespace MasterNet.Client.Courses;

internal static class ApiErrors
{
    public static string Describe(HttpStatusCode statusCode, string? content)
    {
        switch (statusCode)
        {
            case HttpStatusCode.Unauthorized:
                return "You must sign in to do this.";
            case HttpStatusCode.Forbidden:
                return "Your user does not have permission to do this.";
            case HttpStatusCode.NotFound:
                return "The course does not exist.";
        }

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m));

                    return string.Join(" ", messages);
                }

                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                {
                    return detail.GetString()!;
                }
            }
            catch (JsonException)
            {
                // cuerpo que no es JSON: se usa el mensaje generico
            }
        }

        return $"The request failed ({(int)statusCode}).";
    }
}
