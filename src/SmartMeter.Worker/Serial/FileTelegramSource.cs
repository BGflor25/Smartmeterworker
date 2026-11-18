using Microsoft.Extensions.Options;
using SmartMeter.Worker.Parsing;

namespace SmartMeter.Worker.Serial;

/// <summary>Development source: replays the last valid telegram from a captured file.</summary>
public sealed class FileTelegramSource(IOptions<MeterOptions> options) : ITelegramSource
{
    public async Task<string?> ReadTelegramAsync(CancellationToken ct)
    {
        var text = await File.ReadAllTextAsync(options.Value.SourceFile!, System.Text.Encoding.Latin1, ct);
        return TelegramFramer.LastValid(text);
    }
}
