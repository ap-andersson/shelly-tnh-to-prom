using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using Prometheus;
using ShellyTnhToProm;
using System.Text;
using System.Text.Json;

Logger.Log("Starting...");


// Read configuration

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var endpoint = configuration["MQTT_ENDPOINT"];
var topicFilter = configuration["MQTT_TOPIC_FILTER"];
var prometheusPort = Convert.ToInt32(configuration["PROMETHEUS_PORT"] ?? "1234");
var printMsgs = Convert.ToBoolean(configuration["PRINT_ALL_MSGS"] ?? "false");

Logger.Log($"Prom port: {prometheusPort}");

// Setup graceful shutdown. Not that it actually works in containers on Linux

var cts = new CancellationTokenSource();

var sigintReceived = false;

Console.CancelKeyPress += (s, e) =>
{
    Logger.Log("Canceling");
    //tcs.SetResult();
    e.Cancel = true;
    sigintReceived = true;
    cts.Cancel();
};

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    if (!sigintReceived)
    {
        Logger.Log("Received SIGTERM");
        //tcs.SetResult();
        cts.Cancel();
    }
    else
    {
        Logger.Log("Received SIGTERM, ignoring it because already processed SIGINT");
    }
};


// Setup prometheus stuff

using var server = new Prometheus.MetricServer(port: prometheusPort);
server.Start();

Logger.Log($"Prometheus server started on port '{prometheusPort}'");

var latestTempGauge = Metrics.CreateGauge("temp", "Latest reported temperature in celcius");
var latestHumidityGauge = Metrics.CreateGauge("humidity", "Latest reported humidity in percent");



// Lets start doing the stuff

var clientId = Guid.NewGuid();

var mqttFactory = new MqttFactory();

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer(endpoint)
        .WithCredentials("andreas", "Hej1234!")
        .WithClientId(clientId.ToString())
        .Build();

    // Setup message handling before connecting so that queued messages
    // are also handled properly. When there is no event handler attached all
    // received messages get lost.
    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        if(printMsgs)
        {
            Logger.Log($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
        }
        
                ShellyPlusHnTSensorGen3Model model = null;
        try
        {
            model = JsonSerializer.Deserialize<ShellyPlusHnTSensorGen3Model>(Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to deserialize data, will ignore");
        }

        if (
            model != null && 
            model?.Params?.Temperature0?.TC != null &&
            model?.Params?.Humidity0?.Rh != null
            )
        {
            Logger.Log($"Temp: '{model.Params.Temperature0.TC.Value}*C'. Humidity: '{model.Params.Humidity0.Rh.Value}%'");
            latestTempGauge.Set(model.Params.Temperature0.TC.Value);
            latestHumidityGauge.Set(model.Params.Humidity0.Rh.Value);
        }

        return Task.CompletedTask;
    };

    await mqttClient.ConnectAsync(mqttClientOptions, cts.Token);

    var mqttSubscribeOptions = mqttFactory
        .CreateSubscribeOptionsBuilder()
        .WithTopicFilter(topicFilter)
        .Build();

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, cts.Token);

    Logger.Log("MQTT client subscribed to topic.");

    while (!cts.IsCancellationRequested)
    {
        // My version of an infinity loop
        // Using the ContinueWith to basically ignore the exeption that is thrown
        // when the cts is cancelled
        await Task.Delay(TimeSpan.FromSeconds(60), cts.Token).ContinueWith(tsk => { });
    }

    Logger.Log("Unsubscribing");
    await mqttClient.UnsubscribeAsync(topicFilter);

    Logger.Log("Disconnecting");
    await mqttClient.DisconnectAsync();

    Logger.Log("Exiting");
}