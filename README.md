# shelly-tnh-to-prom

Reads temperature and humidity from a Shelly H&T (Gen3, and other Gen2+
devices that send the same RPC notifications) over MQTT and exposes them as a
Prometheus endpoint.

The Shelly publishes its status to `<prefix>/events/rpc` every time it wakes
up. This service subscribes to that topic and keeps the latest values.

## Run

```sh
cp .env.example .env   # broker, credentials, topic
docker compose pull && docker compose up -d
curl -s localhost:8888/metrics
```

Images for linux/amd64 and linux/arm64 are built by GitHub Actions and pushed
to `ghcr.io/ap-andersson/shelly-tnh-to-prom`.

## Configuration

Environment variables (see [`.env.example`](.env.example)):

| Variable | Default | |
|---|---|---|
| `MQTT_ENDPOINT` | (required) | broker host name or IP |
| `MQTT_PORT` | `1883` | |
| `MQTT_USERNAME`, `MQTT_PASSWORD` | (none) | |
| `MQTT_TOPIC_FILTER` | (required) | e.g. `shellyhtg3-lr/events/rpc`, or `+/events/rpc` for all Shellys |
| `PROMETHEUS_PORT` | `1234` | port inside the container |
| `PRINT_ALL_MSGS` | `false` | log every received message |

Plain MQTT only (no TLS).

## Metrics

All per-device metrics have a `device` label, which is the first part of the
MQTT topic (the Shelly's MQTT prefix, e.g. `shellyhtg3-lr`).

| Metric | |
|---|---|
| `shelly_temperature_celsius` | |
| `shelly_relative_humidity_percent` | |
| `shelly_battery_percent` | |
| `shelly_battery_volts` | |
| `shelly_wifi_rssi_dbm` | |
| `shelly_last_update_timestamp_seconds` | the Shelly sleeps between reports, so alert on e.g. `time() - this > 3 * <wake-up period>` |
| `shelly_mqtt_connected` | 1 while connected to the broker |
| `shelly_invalid_messages_total` | messages that were not valid JSON |

The service reconnects to the broker by itself if the connection drops.

## Development

```sh
docker build -t shelly-tnh-to-prom:dev .
IMAGE=shelly-tnh-to-prom:dev docker compose up -d
```

> **AI assistance:** parts of this project were created with the help of
> Claude (Anthropic's AI assistant). Commits with AI involvement carry a
> `Co-Authored-By: Claude` trailer.
