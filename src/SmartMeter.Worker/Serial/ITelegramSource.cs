namespace SmartMeter.Worker.Serial;

public interface ITelegramSource
{
    /// <summary>Returns one complete, CRC-valid telegram, or null if none arrived in time.</summary>
    Task<string?> ReadTelegramAsync(CancellationToken ct);
}
