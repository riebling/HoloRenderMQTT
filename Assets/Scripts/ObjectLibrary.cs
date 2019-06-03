using System;
using UnityEngine;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class ObjectLibrary : MonoBehaviour
{
    // map render strings to GameObjects
    private Dictionary<string, GameObject> myDict;

    public GameObject Drone;
    public GameObject Fire;
    public GameObject Avatar;
    public GameObject Line;

    private Mqtt myMqtt;
    string received_data;

    public Queue<updateStruct> workQueue;

    public class updateStruct
    {
        public string Name;
        public GameObject gameObj;
        public float X, Y, Z;
        public Boolean hasUpdate;
        public Boolean Enabled;

        public updateStruct(string name, GameObject go, float x, float y, float z, Boolean update, Boolean enabled)
        {
            Name = name;
            gameObj = go;
            X = x;
            Y = y;
            Z = z;
            hasUpdate = update;
            Enabled = enabled;
        }
    }

    public void Start()
    {
        myDict = new Dictionary<string, GameObject>();
        workQueue = new Queue<updateStruct>();

        // map render strings to GameObjects
        /*
                myDict.Add("drone", new updateStruct((ObjectController)Drone.GetComponent<ObjectController>(), 0, 0, 0, false));
                myDict.Add("fire", new updateStruct((ObjectController)Fire.GetComponent<ObjectController>(), 0, 0, 0, false));
                myDict.Add("avatar", new updateStruct((ObjectController)Avatar.GetComponent<ObjectController>(), 0, 0, 0, false));
                */
        myDict.Add("drone", Drone);
        myDict.Add("fire", Fire);
        myDict.Add("avatar", Avatar);
        myDict.Add("line", Line);

        myMqtt = new Mqtt();
        myMqtt.InitMqtt(myMqtt.renderTopic);
        // register a callback-function (we have to implement, see below) which is called by the library when a message was received
        myMqtt.client.MqttMsgPublishReceived += MqttReceiveCallback;
    }

    // MQTT receive callback
    // this code runs when a message is received
    // It can't update objects, that has to be done in the main 'Update()' thread
    public void MqttReceiveCallback(object sender, MqttMsgPublishEventArgs e)
    {
        //System.Diagnostics.Debug.WriteLine("received something");

        received_data = Encoding.UTF8.GetString(e.Message);
        System.Diagnostics.Debug.WriteLine("MQTT: " + received_data);

        // enqueue a position update to be dequeued during Update() time
        // (once per frame)
        string[] splits = received_data.Split(',');
        string name = splits[0];
        //System.Diagnostics.Debug.WriteLine("gameObj is: " + splits[0]);

        float x = float.Parse(splits[1]);
        float y = float.Parse(splits[2]);
        float z = float.Parse(splits[3]);
        string onoff = splits[4];
        //        System.Diagnostics.Debug.WriteLine("x,y,z are: " + x + " " + y + " " + z);
        //        System.Diagnostics.Debug.WriteLine("myDict is: " + myDict.ToString());
        //        System.Diagnostics.Debug.WriteLine("Dict keys are: " + g.name + "," + h.name + "," + i.name);
        //        System.Diagnostics.Debug.WriteLine("gObj is: " + gObj.name);

        Boolean enabled = false;
        if (onoff == "on") enabled = true;

        GameObject go = myDict[name];
        updateStruct us = new updateStruct(name, go, x, y, z, true, enabled);

        workQueue.Enqueue(us);

    }

    private void Update()
    {
        // pull only one location update from the queue
        // we might want to empty out ALL such updates, but hesitant
        // to put a loop inside Update()
        // it would allow to skip/drop multiple updates to same GameObject
        if (workQueue.Count > 0)
        {
            updateStruct us = workQueue.Dequeue();
            GameObject go = us.gameObj;

            if (us.Enabled)
            {
                ObjectController obj = (ObjectController)go.GetComponent<ObjectController>();
                // if previously inactive, warp directly to new coordinate
                if (!go.activeSelf) {
                    go.transform.position = new Vector3(us.X, us.Y, us.Z);
                    go.SetActive(true);
                }
                else {
                    // This could overwrite previously written unused targetXYZs
                    // but because we pull out only 1 per frame (Update()) we get them all
                    obj.targetX = us.X;
                    obj.targetY = us.Y;
                    obj.targetZ = us.Z;
                }
                obj.hasUpdate = true;
            }
            else
            {
                go.SetActive(false);
            }
        }
    }
}
