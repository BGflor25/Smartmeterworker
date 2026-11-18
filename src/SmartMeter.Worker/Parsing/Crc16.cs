using System.Globalization;
using System.Text;

namespace SmartMeter.Worker.Parsing;

/// <summary>
/// CRC-16/ARC (poly 0x8005 reflected = 0xA001, init 0x0000) as used by DSMR 4+/5.
/// Computed over every byte from the leading '/' up to and including the '!'.
/// </summary>
public static class Crc16
{
    public static ushort Compute(ReadOnlySpan<byte> data)
    {
        ushort crc = 0;
        foreach (var b in data)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
                crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
        }
        return crc;
    }

    /// <summary>
    /// Validates a framed telegram ("/...!XXXX"). Telegrams without a CRC
    /// (pre-DSMR 4 meters send just "!") are accepted.
    /// </summary>
    public static bool IsValid(string telegram)
    {
        var bang = telegram.LastIndexOf('!');
        if (bang < 0 || !telegram.StartsWith('/'))
            return false;

        var crcText = telegram[(bang + 1)..].Trim();
        if (crcText.Length == 0)
            return true;

        if (!ushort.TryParse(crcText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var expected))
            return false;

        var bytes = Encoding.Latin1.GetBytes(telegram[..(bang + 1)]);
        return Compute(bytes) == expected;
    }
}
