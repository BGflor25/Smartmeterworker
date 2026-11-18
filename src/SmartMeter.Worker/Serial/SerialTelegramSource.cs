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

        // SerialPort reports a missing device as "Access denied", which is misleading.
        if (!File.Exists(o.PortName))
        {
            logger.LogError(
                "Serial port {Port} not found. Is the P1 cable plugged in and the device mapped into the container (devices:)?",
                o.PortName);
            return null;
        }

        using var port = new SerialPort(o.PortName, o.BaudRate, Parity.None, 8, StopBits.One)
        {
            Encoding = Encoding.Latin1, // byte-preserving, needed for the CRC
            NewLine = "\n",
            ReadTimeout = 1000,
            DtrEnable = o.DtrEnable, // P1 data-request line on many cables
            RtsEnable = o.RtsEnable,
        };
        port.Open();
        port.DiscardInBuffer();

        var sw = Stopwatch.StartNew();
        var buffer = new StringBuilder();
        var inTelegram = false;
        var linesReceived = 0;

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
            linesReceived++;

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

        if (linesReceived == 0)
            logger.LogWarning(
                "No data received on {Port} within {Timeout} (check the cable and DTR/RTS: Meter__DtrEnable={Dtr}, Meter__RtsEnable={Rts})",
                o.PortName, o.ReadTimeout, o.DtrEnable, o.RtsEnable);
        else
            logger.LogWarning(
                "Received {Lines} lines but no valid telegram on {Port} within {Timeout} (check the baud rate, {Baud})",
                linesReceived, o.PortName, o.ReadTimeout, o.BaudRate);
        return null;
    }
}
