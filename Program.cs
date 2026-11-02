using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using ShellyTnhToProm;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.UseUtcTimestamp = true;
    o.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ ";
});

// Only export the Shelly metrics, not the .NET runtime / process internals.
Metrics.SuppressDefaultMetrics();

Settings settings;
try
{
    settings = Settings.FromConfiguration(builder.Configuration);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Configuration error: {ex.Message}");
    return 2;
}

builder.Services.AddSingleton(settings);
builder.Services.AddSingleton<ShellyMetrics>();
builder.Services.AddHostedService<MqttWorker>();
builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(10));

var host = builder.Build();
var log = host.Services.GetRequiredService<ILogger<Program>>();
log.LogInformation("Starting {Version}: {Settings}", Environment.GetEnvironmentVariable("APP_VERSION") ?? "dev", settings);

using var metricServer = new MetricServer(port: settings.PrometheusPort);
metricServer.Start();

await host.RunAsync();
return 0;
