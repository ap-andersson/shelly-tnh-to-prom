using System.Text.Json.Serialization;

namespace ShellyTnhToProm;

// Only the parts of the Shelly Gen2+/Gen3 RPC notification (NotifyFullStatus /
// NotifyStatus on <prefix>/events/rpc) that we export. Every field is optional:
// partial NotifyStatus messages only contain the components that changed.

public sealed record ShellyNotification(
    [property: JsonPropertyName("src")] string? Src,
    [property: JsonPropertyName("method")] string? Method,
    [property: JsonPropertyName("params")] ShellyParams? Params);

public sealed record ShellyParams(
    [property: JsonPropertyName("temperature:0")] ShellyTemperature? Temperature,
    [property: JsonPropertyName("humidity:0")] ShellyHumidity? Humidity,
    [property: JsonPropertyName("devicepower:0")] ShellyDevicePower? DevicePower,
    [property: JsonPropertyName("wifi")] ShellyWifi? Wifi);

public sealed record ShellyTemperature([property: JsonPropertyName("tC")] double? Celsius);

public sealed record ShellyHumidity([property: JsonPropertyName("rh")] double? RelativeHumidity);

public sealed record ShellyDevicePower([property: JsonPropertyName("battery")] ShellyBattery? Battery);

public sealed record ShellyBattery(
    [property: JsonPropertyName("V")] double? Volts,
    [property: JsonPropertyName("percent")] double? Percent);

public sealed record ShellyWifi([property: JsonPropertyName("rssi")] double? Rssi);
