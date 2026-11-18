using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SmartMeter.Worker.Api;

public sealed class MeterApiClient(HttpClient http, IOptions<MeterOptions> options, ILogger<MeterApiClient> logger)
{
    public async Task<HttpStatusCode> PostAsync(SmartMeterPayload payload, CancellationToken ct)
    {
        // Buffered body with Content-Length (like the old curl call), not chunked streaming.
        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(options.Value.ApiUrl, content, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("API returned {Status}: {Body}", (int)response.StatusCode, body);
        }
        return response.StatusCode;
    }
}
