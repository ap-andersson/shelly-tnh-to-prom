using System.Text.Json.Serialization;

namespace ShellyTnhToProm;

public class ShellyPlusHnTSensorGen3Model
{
    [JsonPropertyName("src")]
    public string Src { get; set; }

    [JsonPropertyName("dst")]
    public string Dst { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("params")]
    public Params Params { get; set; }
}

public class Battery
{
    [JsonPropertyName("V")]
    public double? V { get; set; }

    [JsonPropertyName("percent")]
    public int? Percent { get; set; }
}

public class Ble
{
}

public class Cloud
{
    [JsonPropertyName("connected")]
    public bool? Connected { get; set; }
}

public class Devicepower0
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("battery")]
    public Battery Battery { get; set; }

    [JsonPropertyName("external")]
    public External External { get; set; }
}

public class External
{
    [JsonPropertyName("present")]
    public bool? Present { get; set; }
}

public class Humidity0
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("rh")]
    public double? Rh { get; set; }
}

public class Mqtt
{
    [JsonPropertyName("connected")]
    public bool? Connected { get; set; }
}

public class Params
{
    [JsonPropertyName("ts")]
    public double? Ts { get; set; }

    [JsonPropertyName("ble")]
    public Ble Ble { get; set; }

    [JsonPropertyName("cloud")]
    public Cloud Cloud { get; set; }

    [JsonPropertyName("devicepower:0")]
    public Devicepower0 Devicepower0 { get; set; }

    [JsonPropertyName("humidity:0")]
    public Humidity0 Humidity0 { get; set; }

    [JsonPropertyName("mqtt")]
    public Mqtt Mqtt { get; set; }

    [JsonPropertyName("sys")]
    public Sys Sys { get; set; }

    [JsonPropertyName("temperature:0")]
    public Temperature0 Temperature0 { get; set; }

    [JsonPropertyName("wifi")]
    public Wifi Wifi { get; set; }

    [JsonPropertyName("ws")]
    public Ws Ws { get; set; }
}

public class Sys
{
    [JsonPropertyName("mac")]
    public string Mac { get; set; }

    [JsonPropertyName("restart_required")]
    public bool? RestartRequired { get; set; }

    [JsonPropertyName("time")]
    public object Time { get; set; }

    [JsonPropertyName("unixtime")]
    public object Unixtime { get; set; }

    [JsonPropertyName("uptime")]
    public int? Uptime { get; set; }

    [JsonPropertyName("ram_size")]
    public int? RamSize { get; set; }

    [JsonPropertyName("ram_free")]
    public int? RamFree { get; set; }

    [JsonPropertyName("fs_size")]
    public int? FsSize { get; set; }

    [JsonPropertyName("fs_free")]
    public int? FsFree { get; set; }

    [JsonPropertyName("cfg_rev")]
    public int? CfgRev { get; set; }

    [JsonPropertyName("kvs_rev")]
    public int? KvsRev { get; set; }

    [JsonPropertyName("webhook_rev")]
    public int? WebhookRev { get; set; }

    [JsonPropertyName("wakeup_reason")]
    public WakeupReason WakeupReason { get; set; }

    [JsonPropertyName("wakeup_period")]
    public int? WakeupPeriod { get; set; }

    [JsonPropertyName("reset_reason")]
    public int? ResetReason { get; set; }
}

public class Temperature0
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("tC")]
    public double? TC { get; set; }

    [JsonPropertyName("tF")]
    public double? TF { get; set; }
}

public class WakeupReason
{
    [JsonPropertyName("boot")]
    public string Boot { get; set; }

    [JsonPropertyName("cause")]
    public string Cause { get; set; }
}

public class Wifi
{
    [JsonPropertyName("sta_ip")]
    public string StaIp { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("ssid")]
    public string Ssid { get; set; }

    [JsonPropertyName("rssi")]
    public int? Rssi { get; set; }
}

public class Ws
{
    [JsonPropertyName("connected")]
    public bool? Connected { get; set; }
}


