using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using OrchardCore.DynamicFields.Settings;

namespace OrchardCore.DynamicFields.Services;

public class DynamicFieldsHttpClient
{
    private readonly HttpClient _client;
    private readonly ILogger _logger;

    public DynamicFieldsHttpClient(HttpClient client, ILogger<DynamicFieldsHttpClient> logger)
    {
        _client = client;
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _logger = logger;
    }

    public async Task<(bool Success, DynamicFieldSettings Settings)> TryFetchSettingsOrDefaultAsync(string url)
    {
        DynamicFieldSettings settings = default;

        try
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<DynamicFieldSettings>();
            if (content != null)
            {
                settings = content;
                return (true, settings);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error fetching URL: {Url}", url);
        }

        return (false, settings);
    }
}
