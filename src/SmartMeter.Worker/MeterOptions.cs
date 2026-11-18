namespace SmartMeter.Worker;

public sealed class MeterOptions
{
    public const string SectionName = "Meter";

    /// <summary>Serial device of the P1 cable inside the container (or host).</summary>
    public string PortName { get; set; } = "/dev/ttyUSB0";

    /// <summary>DSMR 5 / Fluvius e-MUCS uses 115200 8N1.</summary>
    public int BaudRate { get; set; } = 115200;

    /// <summary>
    /// Raise DTR when opening the port. Many P1 cables power the meter's data-request line
    /// from DTR/RTS; with them low the meter sends nothing. .NET lowers both by default.
    /// </summary>
    public bool DtrEnable { get; set; } = true;

    /// <summary>Raise RTS when opening the port. See <see cref="DtrEnable"/>.</summary>
    public bool RtsEnable { get; set; } = true;

    /// <summary>How long to wait for one complete, CRC-valid telegram per cycle.</summary>
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>Time between readings.</summary>
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Endpoint the JSON payload is POSTed to.</summary>
    public Uri? ApiUrl { get; set; }

    /// <summary>
    /// Development only: read telegrams from this file instead of the serial port
    /// (e.g. an output.txt captured on the Pi).
    /// </summary>
    public string? SourceFile { get; set; }
}
