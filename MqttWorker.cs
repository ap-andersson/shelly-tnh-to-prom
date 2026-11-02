using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace ShellyTnhToProm;

/// <summary>
/// Keeps an MQTT connection to the broker (reconnecting whenever it drops),
/// subscribes to the Shelly topic and turns messages into Prometheus metrics.
/// </summary>
public sealed class MqttWorker(Settings settings, ShellyMetrics metrics, ILogger<MqttWorker> log) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(60);

    private readonly IMqttClient _client = new MqttClientFactory().CreateMqttClient();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.ApplicationMessageReceivedAsync += e =>
        {
            HandleMessage(e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        };
        _client.DisconnectedAsync += e =>
        {
            metrics.MqttConnected.Set(0);
            if (e.ClientWasConnected && !stoppingToken.IsCancellationRequested)
            {
                log.LogWarning("Disconnected from MQTT broker ({Reason}), will reconnect", e.Reason);
            }
            return Task.CompletedTask;
        };

        var options = BuildOptions();
        var backoff = TimeSpan.FromSeconds(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_client.IsConnected)
            {
                try
                {
                    await ConnectAndSubscribeAsync(options, stoppingToken);
                    backoff = TimeSpan.FromSeconds(1);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    log.LogWarning("Could not connect to MQTT broker {Host}:{Port}: {Error}. Retrying in {Delay}s",
                        settings.MqttHost, settings.MqttPort, ex.Message, backoff.TotalSeconds);
                    await Delay(backoff, stoppingToken);
                    backoff = TimeSpan.FromTicks(Math.Min(backoff.Ticks * 2, MaxBackoff.Ticks));
                    continue;
                }
            }

            await Delay(CheckInterval, stoppingToken);
        }
    }

    private MqttClientOptions BuildOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(settings.MqttHost, settings.MqttPort)
            .WithClientId($"shelly-tnh-to-prom-{Guid.NewGuid():N}")
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .WithTimeout(TimeSpan.FromSeconds(10));

        if (settings.MqttUsername is not null)
        {
            builder = builder.WithCredentials(settings.MqttUsername, settings.MqttPassword);
        }

        return builder.Build();
    }

    private async Task ConnectAndSubscribeAsync(MqttClientOptions options, CancellationToken ct)
    {
        await _client.ConnectAsync(options, ct);

        var subscribe = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(settings.MqttTopicFilter)
            .Build();
        await _client.SubscribeAsync(subscribe, ct);

        metrics.MqttConnected.Set(1);
        log.LogInformation("Connected to MQTT broker {Host}:{Port}, subscribed to {Topic}",
            settings.MqttHost, settings.MqttPort, settings.MqttTopicFilter);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_client.IsConnected)
        {
            log.LogInformation("Disconnecting from MQTT broker");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            try
            {
                await _client.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().Build(), timeout.Token);
            }
            catch (Exception ex)
            {
                log.LogDebug("Ignoring error while disconnecting: {Error}", ex.Message);
            }
        }
        metrics.MqttConnected.Set(0);
    }

    public override void Dispose()
    {
        _client.Dispose();
        base.Dispose();
    }

    private void HandleMessage(string topic, string payload)
    {
        if (settings.PrintAllMessages)
        {
            log.LogInformation("Message on {Topic}: {Payload}", topic, payload);
        }

        ShellyNotification? message;
        try
        {
            message = JsonSerializer.Deserialize<ShellyNotification>(payload);
        }
        catch (JsonException ex)
        {
            metrics.InvalidMessages.Inc();
            log.LogWarning("Ignoring message on {Topic} that is not valid JSON: {Error}", topic, ex.Message);
            return;
        }

        var p = message?.Params;
        if (p is null)
        {
            return; // Other RPC notifications (events without status) are fine to skip.
        }

        // The device label is the Shelly's MQTT topic prefix (e.g. "shellyhtg3-lr"),
        // which is the name the user configured on the device.
        var device = topic.Split('/')[0];

        var temperature = p.Temperature?.Celsius;
        var humidity = p.Humidity?.RelativeHumidity;

        if (temperature is { } t) metrics.Temperature.WithLabels(device).Set(t);
        if (humidity is { } h) metrics.Humidity.WithLabels(device).Set(h);
        if (p.DevicePower?.Battery?.Percent is { } bp) metrics.BatteryPercent.WithLabels(device).Set(bp);
        if (p.DevicePower?.Battery?.Volts is { } bv) metrics.BatteryVolts.WithLabels(device).Set(bv);
        if (p.Wifi?.Rssi is { } rssi) metrics.WifiRssi.WithLabels(device).Set(rssi);

        if (temperature is not null || humidity is not null)
        {
            metrics.LastUpdate.WithLabels(device).Set(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0);
            var parts = new List<string>();
            if (temperature is not null) parts.Add($"temperature={temperature}°C");
            if (humidity is not null) parts.Add($"humidity={humidity}%");
            if (p.DevicePower?.Battery?.Percent is { } battery) parts.Add($"battery={battery}%");
            log.LogInformation("{Device}: {Values}", device, string.Join(" ", parts));
        }
    }

    private static async Task Delay(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
    }
}
