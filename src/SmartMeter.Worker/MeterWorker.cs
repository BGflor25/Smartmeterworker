using Microsoft.Extensions.Options;
using SmartMeter.Worker.Api;
using SmartMeter.Worker.Parsing;
using SmartMeter.Worker.Serial;

namespace SmartMeter.Worker;

public sealed class MeterWorker(
    ITelegramSource source,
    MeterApiClient api,
    IOptions<MeterOptions> options,
    ILogger<MeterWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Reading {Source} every {Interval}, posting to {Url}",
            options.Value.SourceFile ?? options.Value.PortName, options.Value.Interval, options.Value.ApiUrl);

        using var timer = new PeriodicTimer(options.Value.Interval);
        try
        {
            do
            {
                await RunOnceAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            var raw = await source.ReadTelegramAsync(ct);
            if (raw is null)
                return;

            var telegram = DsmrParser.Parse(raw);
            var payload = PayloadBuilder.Build(telegram);
            var status = await api.PostAsync(payload, ct);

            logger.LogInformation(
                "Meter {Time:yyyy-MM-dd HH:mm:sszzz}: usage {Usage} kW, delivery {Delivery} kW, gas {Gas} m3 -> HTTP {Status}",
                DsmrTimestamp.Parse(telegram.GetRaw("0-0:1.0.0")),
                payload.CurrentPowerUsage.Value, payload.CurrentPowerDelivery.Value,
                payload.GasConsumption, (int)status);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Meter cycle failed");
        }
    }
}
