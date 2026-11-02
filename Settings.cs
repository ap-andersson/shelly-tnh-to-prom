using Microsoft.Extensions.Configuration;

namespace ShellyTnhToProm;

/// <summary>
/// All configuration, read from environment variables (or appsettings.json for
/// non-secret defaults). Names are kept compatible with earlier versions.
/// </summary>
public sealed record Settings(
    string MqttHost,
    int MqttPort,
    string? MqttUsername,
    string? MqttPassword,
    string MqttTopicFilter,
    int PrometheusPort,
    bool PrintAllMessages)
{
    public static Settings FromConfiguration(IConfiguration config)
    {
        var host = config["MQTT_ENDPOINT"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("MQTT_ENDPOINT must be set (host name or IP of the MQTT broker)");
        }

        var topic = config["MQTT_TOPIC_FILTER"];
        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new InvalidOperationException("MQTT_TOPIC_FILTER must be set, e.g. shellyhtg3-lr/events/rpc");
        }

        var username = config["MQTT_USERNAME"];
        var password = config["MQTT_PASSWORD"];

        return new Settings(
            MqttHost: host.Trim(),
            MqttPort: ParseInt(config, "MQTT_PORT", 1883),
            MqttUsername: string.IsNullOrEmpty(username) ? null : username,
            MqttPassword: string.IsNullOrEmpty(password) ? null : password,
            MqttTopicFilter: topic.Trim(),
            PrometheusPort: ParseInt(config, "PROMETHEUS_PORT", 1234),
            PrintAllMessages: ParseBool(config, "PRINT_ALL_MSGS", false));
    }

    private static int ParseInt(IConfiguration config, string key, int fallback)
    {
        var raw = config[key];
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        return int.TryParse(raw, out var value)
            ? value
            : throw new InvalidOperationException($"{key} must be an integer, got '{raw}'");
    }

    private static bool ParseBool(IConfiguration config, string key, bool fallback)
    {
        var raw = config[key];
        if (string.IsNullOrWhiteSpace(raw)) return fallback;
        return bool.TryParse(raw, out var value)
            ? value
            : throw new InvalidOperationException($"{key} must be true or false, got '{raw}'");
    }

    /// <summary>For the startup log: everything except the password.</summary>
    public override string ToString() =>
        $"broker={MqttHost}:{MqttPort} user={MqttUsername ?? "(none)"} topic={MqttTopicFilter} " +
        $"prometheusPort={PrometheusPort} printAllMessages={PrintAllMessages}";
}
