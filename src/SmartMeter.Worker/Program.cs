using Microsoft.Extensions.Options;
using SmartMeter.Worker;
using SmartMeter.Worker.Api;
using SmartMeter.Worker.Serial;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<MeterOptions>()
    .Bind(builder.Configuration.GetSection(MeterOptions.SectionName))
    .Validate(o => o.ApiUrl is not null, "Meter:ApiUrl is required")
    .Validate(o => o.Interval > TimeSpan.Zero, "Meter:Interval must be positive")
    .Validate(o => o.SourceFile is not null || !string.IsNullOrWhiteSpace(o.PortName), "Meter:PortName is required")
    .ValidateOnStart();

builder.Services.AddSingleton<ITelegramSource>(sp =>
    string.IsNullOrWhiteSpace(sp.GetRequiredService<IOptions<MeterOptions>>().Value.SourceFile)
        ? ActivatorUtilities.CreateInstance<SerialTelegramSource>(sp)
        : ActivatorUtilities.CreateInstance<FileTelegramSource>(sp));

builder.Services.AddHttpClient<MeterApiClient>()
    .AddStandardResilienceHandler();

builder.Services.AddHostedService<MeterWorker>();

builder.Build().Run();
