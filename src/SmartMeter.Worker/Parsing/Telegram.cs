using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartMeter.Worker.Parsing;

/// <summary>A parsed DSMR telegram: OBIS code → the raw parenthesized groups of that line.</summary>
public sealed partial class Telegram
{
    private readonly Dictionary<string, string[]> _lines;

    public Telegram(string? header, string? checksum, Dictionary<string, string[]> lines)
    {
        Header = header;
        Checksum = checksum;
        _lines = lines;
    }

    public string? Header { get; }
    public string? Checksum { get; }
    public IReadOnlyDictionary<string, string[]> Lines => _lines;

    /// <summary>The last group of a line (multi-group lines like gas carry timestamp + value).</summary>
    public string? GetRaw(string obis) =>
        _lines.TryGetValue(obis, out var groups) && groups.Length > 0 ? groups[^1] : null;

    /// <summary>The last group as a number with its "*unit" suffix stripped, or null.</summary>
    public double? GetNumber(string obis)
    {
        var raw = GetRaw(obis);
        if (raw is null)
            return null;

        var match = NumberRegex().Match(raw);
        return match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
            ? v
            : null;
    }

    /// <summary>Hex-encoded ASCII (meter serials) decoded to text, or the raw value if it isn't hex.</summary>
    public string? GetHexText(string obis)
    {
        var raw = GetRaw(obis);
        if (string.IsNullOrEmpty(raw))
            return raw;
        try
        {
            var bytes = Convert.FromHexString(raw);
            return bytes.All(b => b < 0x80) ? System.Text.Encoding.ASCII.GetString(bytes) : raw;
        }
        catch (FormatException)
        {
            return raw;
        }
    }

    [GeneratedRegex(@"^([0-9.]+)(\*[a-zA-Z0-9]+)?$")]
    private static partial Regex NumberRegex();
}
