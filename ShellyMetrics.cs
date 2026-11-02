using Prometheus;

namespace ShellyTnhToProm;

/// <summary>Prometheus metrics. Per-device series only appear once that device has reported.</summary>
public sealed class ShellyMetrics
{
    private static readonly string[] DeviceLabel = ["device"];

    public Gauge Temperature { get; } = Metrics.CreateGauge(
        "shelly_temperature_celsius", "Temperature reported by the Shelly, in degrees Celsius.", DeviceLabel);

    public Gauge Humidity { get; } = Metrics.CreateGauge(
        "shelly_relative_humidity_percent", "Relative humidity reported by the Shelly, in percent.", DeviceLabel);

    public Gauge BatteryPercent { get; } = Metrics.CreateGauge(
        "shelly_battery_percent", "Battery level reported by the Shelly, in percent.", DeviceLabel);

    public Gauge BatteryVolts { get; } = Metrics.CreateGauge(
        "shelly_battery_volts", "Battery voltage reported by the Shelly.", DeviceLabel);

    public Gauge WifiRssi { get; } = Metrics.CreateGauge(
        "shelly_wifi_rssi_dbm", "Wi-Fi signal strength reported by the Shelly, in dBm.", DeviceLabel);

    public Gauge LastUpdate { get; } = Metrics.CreateGauge(
        "shelly_last_update_timestamp_seconds",
        "Unix time of the last message with measurements from the Shelly.", DeviceLabel);

    public Counter InvalidMessages { get; } = Metrics.CreateCounter(
        "shelly_invalid_messages_total", "Messages on the subscribed topic that could not be parsed.");

    public Gauge MqttConnected { get; } = Metrics.CreateGauge(
        "shelly_mqtt_connected", "1 if connected to the MQTT broker.");
}
