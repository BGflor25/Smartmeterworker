using System.Text.RegularExpressions;

namespace SmartMeter.Worker.Parsing;

public static partial class DsmrParser
{
    /// <summary>Parses one framed telegram. Later lines with the same OBIS code overwrite earlier ones.</summary>
    public static Telegram Parse(string telegram)
    {
        string? header = null, checksum = null;
        var lines = new Dictionary<string, string[]>();

        foreach (var rawLine in telegram.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
                continue;
            if (line.StartsWith('!'))
            {
                checksum = line[1..];
                continue;
            }
            if (line.StartsWith('/'))
            {
                header = line;
                continue;
            }

            var match = LineRegex().Match(line);
            if (!match.Success)
                continue;

            lines[match.Groups[1].Value] = GroupRegex().Matches(match.Groups[2].Value)
                .Select(m => m.Groups[1].Value)
                .ToArray();
        }

        return new Telegram(header, checksum, lines);
    }

    [GeneratedRegex(@"^([0-9\-:.]+)((?:\([^)]*\))+)$")]
    private static partial Regex LineRegex();

    [GeneratedRegex(@"\(([^)]*)\)")]
    private static partial Regex GroupRegex();
}
