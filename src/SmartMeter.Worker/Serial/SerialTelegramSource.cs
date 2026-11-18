using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Options;
using SmartMeter.Worker.Parsing;

namespace SmartMeter.Worker.Serial;

/// <summary>
/// Opens the P1 port, waits for the start of a telegram ('/'), reads until the "!XXXX"
/// line and validates the CRC. Invalid telegrams are skipped; the meter sends a new
/// one every second, so the next one is tried until ReadTimeout expires.
/// </summary>
public sealed class SerialTelegramSource(IOptions<MeterOptions> options, ILogger<SerialTelegramSource> logger)
    : ITelegramSource
{
    public Task<string?> ReadTelegramAsync(CancellationToken ct) =>
        Task.Run(() => Read(ct), ct);

    private string? Read(CancellationToken ct)
    {
        var o = options.Value;
        using var port = new SerialPort(o.PortName, o.BaudRate, Parity.None, 8, StopBits.One)
        {
            Encoding = Encoding.Latin1, // byte-preserving, needed for the CRC
            NewLine = "\n",
            ReadTimeout = 1000,
        };
        port.Open();
        port.DiscardInBuffer();

        var sw = Stopwatch.StartNew();
        var buffer = new StringBuilder();
        var inTelegram = false;

        while (sw.Elapsed < o.ReadTimeout)
        {
            ct.ThrowIfCancellationRequested();

            string line;
            try
            {
                line = port.ReadLine(); // keeps the trailing '\r'
            }
            catch (TimeoutException)
            {
                continue;
            }

            if (line.StartsWith('/'))
            {
                buffer.Clear();
                inTelegram = true;
            }
            if (!inTelegram)
                continue;

            if (line.StartsWith('!'))
            {
                var telegram = buffer.Append(line.TrimEnd('\r')).ToString();
                if (Crc16.IsValid(telegram))
                    return telegram;

                logger.LogWarning("Discarding telegram with invalid CRC ({Checksum})", line.Trim());
                buffer.Clear();
                inTelegram = false;
                continue;
            }

            buffer.Append(line).Append('\n');
        }

        logger.LogWarning("No valid telegram received on {Port} within {Timeout}", o.PortName, o.ReadTimeout);
        return null;
    }
}
