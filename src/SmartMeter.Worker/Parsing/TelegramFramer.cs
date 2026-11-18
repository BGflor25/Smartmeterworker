namespace SmartMeter.Worker.Parsing;

/// <summary>Finds complete telegrams ('/' at line start through the "!XXXX" line) in raw text.</summary>
public static class TelegramFramer
{
    public static IEnumerable<string> Frames(string text)
    {
        var lines = text.Split('\n');
        List<string>? current = null;

        foreach (var line in lines)
        {
            if (line.StartsWith('/'))
            {
                current = [line];
                continue;
            }
            if (current is null)
                continue;

            if (line.StartsWith('!'))
            {
                current.Add(line.TrimEnd('\r'));
                yield return string.Join('\n', current);
                current = null;
                continue;
            }
            current.Add(line);
        }
    }

    /// <summary>Returns the last complete telegram whose CRC is valid, or null.</summary>
    public static string? LastValid(string text) =>
        Frames(text).LastOrDefault(Crc16.IsValid);
}
