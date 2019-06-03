using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadsetController : MonoBehaviour
{
    private int framecounter;
    Mqtt myMqtt;
    string resolutionFormat = "{0:.###}"; // millimeter resolution
    Transform cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // at 60fps, every 6 frames = 10 fps update
        if ((framecounter++) % 6 == 0)
        {
            // MQTT send head coordinates (or other?) 
            //System.Diagnostics.Debug.WriteLine("Sending...");
            string sendString =
                String.Format(resolutionFormat, cam.position.x) + ',' +
                String.Format(resolutionFormat, cam.position.y) + ',' +
                String.Format(resolutionFormat, cam.position.z) + ',' +
                String.Format(resolutionFormat, cam.rotation.w) + ',' +
                String.Format(resolutionFormat, cam.rotation.x) + ',' +
                String.Format(resolutionFormat, cam.rotation.y) + ',' +
                String.Format(resolutionFormat, cam.rotation.z);

            string time = String.Format(resolutionFormat, (Double) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 100);

            // VIO message format is comma separated: time,x,y,z,dist
            sendString = time +
                String.Format(resolutionFormat, cam.position.x) + ',' +
                String.Format(resolutionFormat, cam.position.y) + ',' +
                String.Format(resolutionFormat, cam.position.z) + ',' +
                "0";

            // Publish headset coordinates
            // myMqtt.Send(myMqtt.headsetTopic, sendString);

            //System.Diagnostics.Debug.WriteLine("Sent: " + sendString);
        }
    }
}
