using System;
using Serilog;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NDC8.ACINET.ACI;

namespace MQTT_Client_SM_Service
{
    class Service
    {
        public Service()
        {


            Log.Information("SERVICE STARTING...");


        }

        public void Start()
        {

            Log.Information("SERVICE STARTED");

            HostHandler host = new HostHandler();
            host.Connect("127.0.0.1", 30001);


            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                            .WithClientId("MQTT_Client_SM")
                            .WithTcpServer("192.168.1.20", 1883);

            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                                    .WithClientOptions(builder.Build())
                                    .Build();



            IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();
            Log.Information("MQTT: CONNECTING... IP: 192.168.1.20 Port: 1883");

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {

                if (e.ApplicationMessage.Payload != null)
                {
                    string str = Encoding.Default.GetString(e.ApplicationMessage.Payload);

                    var ssio = new SSIO();

                    ssio = JsonConvert.DeserializeObject<SSIO>(str);




                    Log.Information("MQTT: RECEIVED MQTT MESSAGE");
                    Log.Information($"MQTT: Topic = {e.ApplicationMessage.Topic}");
                    Log.Information($"MQTT: Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Log.Information($"MQTT: Payload Converted = Line: " + ssio.line + " Unit: " + ssio.unit + " ID: " + ssio.id + " Value: " + ssio.value);
                    Log.Information($"MQTT: QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Log.Information($"MQTT: Retain = {e.ApplicationMessage.Retain}");


                    Message_K_IO k = new Message_K_IO(Convert.ToByte(ssio.line), Convert.ToByte(ssio.unit), Convert.ToByte(ssio.id), Convert.ToByte(ssio.value));
                    VCP9412.ACI_Instance.SendMessage(k);
                    Log.Information($"SM: Message sent");

                }



                //Task.Run(() => _mqttClient.PublishAsync("test/topic/data"));
            });

            _mqttClient.StartAsync(options).GetAwaiter().GetResult();

            _mqttClient.UseConnectedHandler(async e =>
            {
                Log.Information("MQTT: CONNECTED WITH SERVER 192.168.1.20, 1883");

                // Subscribe to a topic
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("test/topic/data").Build());

                Log.Information("MQTT: SUBSCRIBED to test/topic/data");
            });

            //while (true)
            //{
            //    string json = JsonConvert.SerializeObject(new { message = "Heart beat", sent = DateTimeOffset.UtcNow });
            //    _mqttClient.PublishAsync("test/topic/json", json);

            //    Task.Delay(1000).GetAwaiter().GetResult();
            //}
        }

        public void Stop()
        {
            Log.Information("SERVICE STOPPED");
        }

        public static void OnConnected(MqttClientConnectedEventArgs obj)
        {
            Log.Information("MQTT: CONNECTED");
        }

        public static void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            Log.Warning("MQTT: CONNECTION FAILED");
        }

        public static void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Log.Information("MQTT: DISCONNECTED");
        }

        public class SSIO
        {
            public int line { get; set; }
            public int unit { get; set; }
            public int id { get; set; }
            public int value { get; set; }
        }
    }
}
