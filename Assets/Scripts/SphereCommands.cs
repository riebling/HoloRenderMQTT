using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine.XR.WSA;
//using Windows.Perception.Spatial;
//using Windows.Perception.Spatial;

public class SphereCommands : MonoBehaviour
{
    private const int listenPort = 9005;
    UdpClient listener;

    string received_data;
    Boolean anchorSet = false;
    WorldAnchor worldAnchor;

    void Start()
	{
        System.Diagnostics.Debug.WriteLine("Starting...");

        // The simplest way to render world-locked holograms is to create a stationary reference frame
        // when the app is launched. This is roughly analogous to creating a "world" coordinate system
        // with the origin placed at the device's position as the app is launched.
//        SpatialLocator locator = SpatialLocator.GetDefault();
//        SpatialStationaryFrameOfReference referenceFrame = locator.CreateStationaryFrameOfReferenceAtCurrentLocation();
//        System.Diagnostics.Debug.WriteLine(referenceFrame.CoordinateSystem.ToString());

        IPEndPoint ep = new IPEndPoint(IPAddress.Any, listenPort);
      
        listener = new UdpClient(ep);
        System.Diagnostics.Debug.WriteLine(listener.ToString());
        System.Diagnostics.Debug.WriteLine(listener.Client.ToString());

        string hostname = System.Net.Dns.GetHostName();
        System.Diagnostics.Debug.WriteLine(hostname);
        string myIP = System.Net.Dns.GetHostEntry(hostname).AddressList[0].ToString();
        System.Diagnostics.Debug.WriteLine("IP is: "+myIP);

        try {
            IAsyncResult iar = listener.BeginReceive(new AsyncCallback(recv), null);
            System.Diagnostics.Debug.WriteLine("Done initializing, listener" + iar.ToString());
        }
        catch(Exception e) {
            System.Diagnostics.Debug.WriteLine(e.ToString());
        }

        //GameObject g = GameObject.Find("Ethan2");
        //g.SetActive(false);

    }

    float x, y, z = 0;
    Boolean hasUpdate = false;

    private void recv(IAsyncResult res)
    {
        System.Diagnostics.Debug.WriteLine("received something");

        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        byte[] receive_byte_array = listener.EndReceive(res, ref RemoteIpEndPoint);
        received_data = Encoding.UTF8.GetString(receive_byte_array);
        System.Diagnostics.Debug.WriteLine(Encoding.UTF8.GetString(receive_byte_array));

        // parse string
        string[] splits = received_data.Split(',');
        x = float.Parse(splits[0]);
        y = float.Parse(splits[1]);
        z = float.Parse(splits[2]);

        // scale
        int scalingFactor = 10; // 1/10th scale
        x = x / scalingFactor;
        y = y / scalingFactor;
        z = z / scalingFactor;


        System.Diagnostics.Debug.WriteLine("done translating, listening again...");

        hasUpdate = true;
        listener.BeginReceive(new AsyncCallback(recv), null);
    }

    private void Update()
    {
        // if (hasupdate) {
        if (false) {
            float oldx, oldy, oldz;
            //System.Diagnostics.Debug.WriteLine("Translate from " + oldx + " " + oldy + " " + oldz);
            //System.Diagnostics.Debug.WriteLine("About to translate " + x + " " + y + " " + z);

            //Process codes

            oldx = this.gameObject.transform.position.x;
            oldy = this.gameObject.transform.position.y;
            oldz = this.gameObject.transform.position.z;

            //transform.Translate(x, y, z, Space.World);
            transform.position = new Vector3(x, y, z);

            Vector3 oldVec = new Vector3(oldx, oldy, oldz);
            Vector3 newVec = new Vector3(x,y,z);
            Vector3 direction = newVec - oldVec;
            Vector3 onlyZ = new Vector3(0, 0, direction.z);

            transform.rotation = Quaternion.FromToRotation(Vector3.forward, newVec - oldVec);

            Text myText = GameObject.Find("Canvas").GetComponent<Text>();
            myText.text = oldx + Environment.NewLine + 
                oldy + Environment.NewLine +
                oldz;

            hasUpdate = false;
        }
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        System.Diagnostics.Debug.WriteLine("selected");
        GetComponent<AudioSource>().Play();

        // Get (parent) world transform
        GameObject parentObject = GameObject.Find("OrigamiCollection");

        /*
        if (anchorSet) {
            Destroy(parentObject.GetComponent<WorldAnchor>());
            anchorSet = false;
        }
        else {
            worldAnchor = parentObject.AddComponent<WorldAnchor>();
            anchorSet = true;
        }
        */

        // Set World Origin to 3d axis 1m in front of headset position
        parentObject.transform.position = transform.position;
        parentObject.transform.rotation = transform.rotation;

        // turn on avatar
        GameObject g = GameObject.Find("Ethan2");
        g.SetActive(true);

        // Move to origin first 
        g = GameObject.Find("Sphere2");
        g.transform.position = transform.position;
        g.transform.rotation = transform.rotation;

        // turn off xyz frame of reference item
        g = GameObject.Find("xyzFrame");
        g.SetActive(false);
        g = GameObject.Find("xyzFrame2");
        g.SetActive(false);
        // turn off the paper ball 'sphere'
        this.GetComponent<Renderer>().enabled = false;

        // Adding a Rigidbody component turns on physics.
        //        if (!this.GetComponent<Rigidbody>())
        //        {
        //            var rigidbody = this.gameObject.AddComponent<Rigidbody>();
        //            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //        }
    }
}
