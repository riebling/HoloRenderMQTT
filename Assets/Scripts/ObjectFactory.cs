using System;
using UnityEngine;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

public class ObjectFactory : MonoBehaviour
{
    // map render strings to GameObjects
    private Dictionary<string, GameObject> myDict;

    private Dictionary<String, GameObject> worldObjects;

    public GameObject Drone;
    public GameObject Fire;
    public GameObject Avatar;
    public GameObject Line;

    private Mqtt myMqtt;
    string received_data;
    private char[] trimChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '.', '_' };

    public Queue<updateStruct> workQueue;

    public class updateStruct
    {
        public string worldName;
        public string baseName;
        public float X, Y, Z;
        public float Xrot, Yrot, Zrot, Wrot;
        public Boolean hasUpdate;
        public Boolean Enabled;

        public updateStruct(string name, string go, float x, float y, float z, Boolean update, Boolean enabled)
        {
            worldName = name;
            baseName = go;
            X = x;
            Y = y;
            Z = z;
            Xrot = 0;
            Yrot = 0;
            Zrot = 0;
            Wrot = 0;
            hasUpdate = update;
            Enabled = enabled;
        }

        public updateStruct(string name, string go, float x, float y, float z, 
            float xrot, float yrot, float zrot, float wrot, Boolean update, Boolean enabled)
        {
            worldName = name;
            baseName = go;
            X = x;
            Y = y;
            Z = z;
            Xrot = xrot;
            Yrot = yrot;
            Zrot = zrot;
            Wrot = wrot;
            hasUpdate = update;
            Enabled = enabled;
        }
    }

    public void Start()
    {
        myDict = new Dictionary<string, GameObject>();
        workQueue = new Queue<updateStruct>();

        worldObjects = new Dictionary<string, GameObject>();

        // map render strings to GameObjects

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
        //System.Diagnostics.Debug.WriteLine("MQTT: " + received_data);

        // enqueue a position update to be dequeued during Update() time
        // (once per frame)
        string[] splits = received_data.Split(',');
        if (splits.Length < 8)
        {
            System.Diagnostics.Debug.WriteLine("Bad MQTT message: " + received_data);
            return;
        }
        string worldname = splits[0];
        // strip extra non-component-name bits from world name
        // e.g. "drone1" -> "drone", "avatar-2" -> "avatar", "fire_3" -> "fire"
        String basename = worldname.Trim(trimChars);

        //System.Diagnostics.Debug.WriteLine("baseName is: " + splits[0]);
        float x = float.Parse(splits[1]);
        float y = float.Parse(splits[2]);
        float z = float.Parse(splits[3]);
        float xrot = float.Parse(splits[4]);
        float yrot = float.Parse(splits[5]);
        float zrot = float.Parse(splits[6]);
        float wrot = float.Parse(splits[7]);
        string onoff = splits[8];
        //        System.Diagnostics.Debug.WriteLine("x,y,z are: " + x + " " + y + " " + z);
        //        System.Diagnostics.Debug.WriteLine("myDict is: " + myDict.ToString());
        //        System.Diagnostics.Debug.WriteLine("Dict keys are: " + g.name + "," + h.name + "," + i.name);
        //        System.Diagnostics.Debug.WriteLine("gObj is: " + gObj.name);

        Boolean enabled = false;
        if (onoff == "on") enabled = true;

        //GameObject go = myDict[name];
        updateStruct us = new updateStruct(worldname, basename, x, y, z, xrot, yrot, zrot,wrot, true, enabled);

        workQueue.Enqueue(us);
        // dump stale updates
        while (workQueue.Count > 5)
            workQueue.Dequeue();

    }

    private void Update()
    {
        // pull only one location update from the queue
        // we might want to empty out ALL such updates, but hesitant
        // to put a loop inside Update()
        // it would allow to skip/drop multiple updates to same GameObject
        if (workQueue.Count > 0)
        {
            //System.Diagnostics.Debug.WriteLine("Qcount: " + workQueue.Count);
            updateStruct us = workQueue.Dequeue();
            GameObject go;

            // where Name is something like "drone1" "avatar-2" "fire_3" etc.
            if (!worldObjects.ContainsKey(us.worldName))
            {
                // Instantiate new GameObject
                // caveat: Start() may not get called for newly instantiated GameObject scripts!

                go = Instantiate(myDict[us.baseName], transform);
                go.name = us.worldName;
                worldObjects.Add(us.worldName, go);
                ObjectController obj = (ObjectController)go.GetComponent<ObjectController>();
                //us.baseName = go;
                us.Enabled = true;
                obj.hasUpdate = true;
                obj.us = us;
                go.transform.position = new Vector3(us.X, us.Y, us.Z);
                go.transform.rotation = new Quaternion(us.Xrot, us.Yrot, us.Zrot, us.Wrot);
                go.SetActive(true);
            }
            else
            {
                // Update existing GameObject
                go = worldObjects[us.worldName];
                ObjectController obj = (ObjectController)go.GetComponent<ObjectController>();
                if (us.Enabled == true)
                {
                    //us.baseName = go;
                    us.Enabled = true;
                    obj.hasUpdate = true;
                    obj.us = us;
                    go.SetActive(true);
                }
                else
                {
                    Destroy(go);
                    worldObjects.Remove(us.worldName);
                }
            }

            if (us.Enabled)
            {
                ObjectController obj = (ObjectController)go.GetComponent<ObjectController>();
                // if previously inactive, warp directly to new coordinate

                obj.targetX = us.X;
                obj.targetY = us.Y;
                obj.targetZ = us.Z;
                obj.targetXrot = us.Xrot;
                obj.targetYrot = us.Yrot;
                obj.targetZrot = us.Zrot;
                obj.targetWrot = us.Wrot;
                obj.hasUpdate = true;
            }
            else
            {
                go.SetActive(false);
            }
        }
    }
}
