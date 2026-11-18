using System.Globalization;
using System.Text.RegularExpressions;

namespace SmartMeter.Worker.Parsing;

public static partial class DsmrTimestamp
{
    /// <summary>
    /// Parses a DSMR timestamp "YYMMDDhhmmssX" where X is 'S' (summer, UTC+2 in Belgium)
    /// or 'W' (winter, UTC+1).
    /// </summary>
    public static DateTimeOffset? Parse(string? value)
    {
        if (value is null)
            return null;

        var m = TimestampRegex().Match(value);
        if (!m.Success)
            return null;

        int Part(int i) => int.Parse(m.Groups[i].Value, CultureInfo.InvariantCulture);
        var offset = TimeSpan.FromHours(m.Groups[7].Value == "S" ? 2 : 1);
        try
        {
            return new DateTimeOffset(2000 + Part(1), Part(2), Part(3), Part(4), Part(5), Part(6), offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>Unix epoch seconds, matching what the Python script sent.</summary>
    public static double? ToUnixSeconds(string? value) => Parse(value)?.ToUnixTimeSeconds();

    [GeneratedRegex(@"^(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})([SW])$")]
    private static partial Regex TimestampRegex();
}
