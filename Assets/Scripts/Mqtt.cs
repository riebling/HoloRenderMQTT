using System;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class Mqtt
{
    // MQTT Stuff
    // string BrokerAddress = "johnpi.local";
    public string BrokerAddress = "oz.andrew.cmu.edu";

    // Subscribe topics
    public string tracker1Topic = "/topic/uwb_range_1";
    public string tracker2Topic = "/topic/uwb_range_2";
    public string renderTopic = "/topic/render";
    public string skeletonTopic = "/topic/skeleton";

    // Publish topics - our coordinates + orientation

    //    public string headsetTopic = "/topic/vio_1"; // this is what John's demo uses
    public string headsetTopic = "/topic/hololens"; // this is what John's demo uses

    public MqttClient client;
    public Boolean hasUpdate;
    public string receiveBuf;

    public void InitMqtt(string topic)
    {
        client = new MqttClient(BrokerAddress);
        hasUpdate = false;
        string clientId;

        // register a callback-function (we have to implement, see below) which is called by the library when a message was received
        //client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        // use a unique id as client id, each time we start the application
        clientId = Guid.NewGuid().ToString();

        System.Diagnostics.Debug.WriteLine("Connecting to mqtt...");
        client.Connect(clientId);

        // subscribe to the topic with QoS 2
        // we need arrays as parameters because we can subscribe to different topics with one call
        System.Diagnostics.Debug.WriteLine("Subscribing to topic: " + topic);
        client.Subscribe(new string[] { topic }, new byte[] { 2 });

        System.Diagnostics.Debug.WriteLine("MQTT client: " + client.ToString());
    }

    // this example code runs when a message is received
    // in fact, the MQTT receive code is in ObjectFactory.cs
    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Message);

        System.Diagnostics.Debug.WriteLine("MQTT internal: " + message);
    }

    // send over MQTT a string
    // typically headset location+orientation updates
    public void Send(string Topic, string Text)
    {
        client.Publish(Topic, Encoding.UTF8.GetBytes(Text), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
    }
}
