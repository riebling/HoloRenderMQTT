using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class OpenPose3dSkeletonFactory : MonoBehaviour
{
    private LineRenderer leftHead;
    private LineRenderer rightHead;
    private LineRenderer torso;
    private LineRenderer leftArm;
    private LineRenderer rightArm;
    private LineRenderer leftLeg;
    private LineRenderer rightLeg;
    private LineRenderer leftFoot;
    private LineRenderer rightFoot;

    private Mqtt myMqtt;

    public GameObject skeletonObject;
    public Queue<SkeletonData> workQueue;

    private Dictionary<String, GameObject> allSkeletons;

    public class SkeletonData
    {
        public String name;
        public GameObject gameObj;
        public Vector3 Nose, Neck, RShoulder, RElbow, RWrist, LShoulder, LElbow, LWrist, MidHip,
         RHip, RKnee, RAnkle, LHip, LKnee, LAnkle,
         REye, LEye, REar, LEar, LBigToe, LSmallToe,
         LHeel, RBigToe, RSmallToe, RHeel, Background;
        public Boolean update, Enabled;

        public SkeletonData(String name_param, GameObject go_param,
            Vector3 Nose_param, Vector3 Neck_param, Vector3 RShoulder_param, Vector3 RElbow_param, 
            Vector3 RWrist_param, Vector3 LShoulder_param, Vector3 LElbow_param, Vector3 LWrist_param, 
            Vector3 MidHip_param, Vector3 RHip_param, Vector3 RKnee_param, Vector3 RAnkle_param, 
            Vector3 LHip_param, Vector3 LKnee_param, Vector3 LAnkle_param, 
            Vector3 REye_param, Vector3 LEye_param, Vector3 REar_param, Vector3 LEar_param, 
            Vector3 LBigToe_param, Vector3 LSmallToe_param, 
            Vector3 LHeel_param, Vector3 RBigToe_param, Vector3 RSmallToe_param, 
            Vector3 RHeel_param, Vector3 Background_param,
            Boolean update_param, Boolean enabled_param)
        {
            name = name_param; gameObj = go_param;
            Nose = Nose_param; Neck = Neck_param; RShoulder = RShoulder_param; RElbow = RElbow_param;
            RWrist = RWrist_param; LShoulder = LShoulder_param; LElbow = LElbow_param; LWrist = LWrist_param;
            MidHip = MidHip_param; RHip = RHip_param; RKnee = RKnee_param; RAnkle = RAnkle_param;
            LHip = LHip_param; LKnee = LKnee_param; LAnkle = LAnkle_param;
            REye = REye_param; LEye = LEye_param; REar = REar_param; LEar = LEar_param;
            LBigToe = LBigToe_param; LSmallToe = LSmallToe_param;
            LHeel = LHeel_param; RBigToe = RBigToe_param; RSmallToe = RSmallToe_param;
            RHeel = RHeel_param; Background = Background_param;
            update = update_param; Enabled = enabled_param;
        }
    }

    private LineRenderer makeRenderer(string theName, int size, Color scolor, Color ecolor)
    {
        GameObject go = new GameObject();
        go.name = theName;
        go.transform.SetParent(transform);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.positionCount = size + 1; // e.g: 1 line = 2 endpoints
        lr.startColor = scolor;
        lr.endColor = ecolor;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;

        // HACK: Unity has a 'feature' where newly created LineRenderers have
        // a default first line from (0,0,0) to (0,0,1) - clear that out!
        lr.SetPosition(1, new Vector3(0, 0, 0));

        return lr;
    }



    // Start is called before the first frame update
    void Start()
    {
        workQueue = new Queue<SkeletonData>();

        allSkeletons = new Dictionary<String, GameObject>();

        myMqtt = new Mqtt();
        myMqtt.InitMqtt(myMqtt.skeletonTopic);
        // register a callback-function (we have to implement, see below) which is called by the library when a message was received
        myMqtt.client.MqttMsgPublishReceived += MqttReceiveCallback;
    }

    // MQTT receive callback
    // this code runs when a message is received
    // It can't update objects, that has to be done in the main 'Update()' thread
    public void MqttReceiveCallback(object sender, MqttMsgPublishEventArgs e)
    {
        //System.Diagnostics.Debug.WriteLine("received something");

        String received_data = Encoding.UTF8.GetString(e.Message);
        //System.Diagnostics.Debug.WriteLine("MQTT: " + received_data);

        // enqueue a position update to be dequeued during Update() time
        // (once per frame)
        string[] splits = received_data.Split(',');
        string name = splits[0];
        //System.Diagnostics.Debug.WriteLine("baseName is: " + splits[0])/100;

        Vector3 Nose = new Vector3(float.Parse(splits[1])/100, float.Parse(splits[2])/100, float.Parse(splits[3])/100);
        Vector3 Neck = new Vector3(float.Parse(splits[4])/100, float.Parse(splits[5])/100, float.Parse(splits[6])/100);
        Vector3 RShoulder = new Vector3(float.Parse(splits[7])/100, float.Parse(splits[8])/100, float.Parse(splits[9])/100);
        Vector3 RElbow = new Vector3(float.Parse(splits[10])/100, float.Parse(splits[11])/100, float.Parse(splits[12])/100);
        Vector3 RWrist = new Vector3(float.Parse(splits[13])/100, float.Parse(splits[14])/100, float.Parse(splits[15])/100);
        Vector3 LShoulder = new Vector3(float.Parse(splits[16])/100, float.Parse(splits[17])/100, float.Parse(splits[18])/100);
        Vector3 LElbow = new Vector3(float.Parse(splits[19])/100, float.Parse(splits[20])/100, float.Parse(splits[21])/100);
        Vector3 LWrist = new Vector3(float.Parse(splits[22])/100, float.Parse(splits[23])/100, float.Parse(splits[24])/100);
        Vector3 MidHip = new Vector3(float.Parse(splits[25])/100, float.Parse(splits[26])/100, float.Parse(splits[27])/100);
        Vector3 RHip = new Vector3(float.Parse(splits[28])/100, float.Parse(splits[29])/100, float.Parse(splits[30])/100);
        Vector3 RKnee = new Vector3(float.Parse(splits[31])/100, float.Parse(splits[32])/100, float.Parse(splits[33])/100);
        Vector3 RAnkle = new Vector3(float.Parse(splits[34])/100, float.Parse(splits[35])/100, float.Parse(splits[36])/100);
        Vector3 LHip = new Vector3(float.Parse(splits[37])/100, float.Parse(splits[38])/100, float.Parse(splits[39])/100);
        Vector3 LKnee = new Vector3(float.Parse(splits[40])/100, float.Parse(splits[41])/100, float.Parse(splits[42])/100);
        Vector3 LAnkle = new Vector3(float.Parse(splits[43])/100, float.Parse(splits[44])/100, float.Parse(splits[45])/100);
        Vector3 REye = new Vector3(float.Parse(splits[46])/100, float.Parse(splits[47])/100, float.Parse(splits[48])/100);
        Vector3 LEye = new Vector3(float.Parse(splits[49])/100, float.Parse(splits[50])/100, float.Parse(splits[51])/100);
        Vector3 REar = new Vector3(float.Parse(splits[52])/100, float.Parse(splits[53])/100, float.Parse(splits[54])/100);
        Vector3 LEar = new Vector3(float.Parse(splits[55])/100, float.Parse(splits[56])/100, float.Parse(splits[57])/100);
        Vector3 LBigToe = new Vector3(float.Parse(splits[58])/100, float.Parse(splits[59])/100, float.Parse(splits[60])/100);
        Vector3 LSmallToe = new Vector3(float.Parse(splits[61])/100, float.Parse(splits[62])/100, float.Parse(splits[63])/100);
        Vector3 LHeel = new Vector3(float.Parse(splits[64])/100, float.Parse(splits[65])/100, float.Parse(splits[66])/100);
        Vector3 RBigToe = new Vector3(float.Parse(splits[67])/100, float.Parse(splits[68])/100, float.Parse(splits[69])/100);
        Vector3 RSmallToe = new Vector3(float.Parse(splits[70])/100, float.Parse(splits[71])/100, float.Parse(splits[72])/100);
        Vector3 RHeel = new Vector3(float.Parse(splits[73])/100, float.Parse(splits[74])/100, float.Parse(splits[75])/100);
        Vector3 Background = new Vector3(0, 0, 0);
        String onoff = splits[76];
        //        Vector3 Background = new Vector3(float.Parse(splits[76])/100, float.Parse(splits[77])/100, float.Parse(splits[78])/100);
        //String onoff = splits[79];

        Boolean enabled = false;
        if (onoff == "on") enabled = true;
        //GameObject go = this.gameObject;
        GameObject go = (GameObject)null;

        SkeletonData sd = new SkeletonData(name, go, 
            Nose, Neck, RShoulder, RElbow, RWrist, 
            LShoulder, LElbow, LWrist, MidHip,
            RHip, RKnee, RAnkle, LHip, LKnee, LAnkle,
            REye, LEye, REar, LEar, LBigToe, LSmallToe,
            LHeel, RBigToe, RSmallToe, RHeel, Background, 
            true, enabled);

        workQueue.Enqueue(sd);
    }

    void tryDraw(LineRenderer lr, Vector3[] positions )
    {
        lr.gameObject.SetActive(true);

        for (int i = positions.Length -1;  i >= 0; i--)
        {
            if (positions[i].x == 0 && positions[i].y == 0 && positions[i].z == 0)
            {
                //lr.gameObject.SetActive(false);
                return;
            }
        }
        lr.SetPositions(positions);
    }

    // Update is called once per frame
    void Update()
    {
        // pull only one location update from the queue
        // we might want to empty out ALL such updates, but hesitant
        // to put a loop inside Update()
        // it would allow to skip/drop multiple updates to same GameObject
        if (workQueue.Count > 0)
        {
            //System.Diagnostics.Debug.WriteLine("Qcount: " + workQueue.Count);
            SkeletonData sd = workQueue.Dequeue();

            if (!allSkeletons.ContainsKey(sd.name))
            {
                // create new Skeleton
                GameObject o = Instantiate(skeletonObject, transform);
                o.name = sd.name;
                allSkeletons.Add(sd.name, o);
                SkeletonScript obj = (SkeletonScript)o.GetComponent<SkeletonScript>();
                sd.gameObj = o;
                sd.Enabled = true;
                obj.hasUpdate = true;
                obj.sd = sd;
                o.SetActive(true);
            }
            else
            {
                GameObject o = allSkeletons[sd.name];
                SkeletonScript obj = (SkeletonScript)o.GetComponent<SkeletonScript>();

                if (sd.Enabled == true)
                {
                    sd.gameObj = o;
                    sd.Enabled = true;
                    obj.hasUpdate = true;
                    obj.sd = sd;
                    o.SetActive(true);
                } else
                {
                    Destroy(o);
                    allSkeletons.Remove(sd.name);
                }
            }
        }

    }
}
