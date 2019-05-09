using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityStandardAssets.Characters.ThirdPerson;

public class SphereCommands2 : MonoBehaviour
{
    private const int listenPort = 9006;
    UdpClient listener;

    string received_data;
    Vector3 origin = new Vector3(0, 0, 0);
    Transform tranny;
    Text myText;
    ThirdPersonCharacter c;
    Animator anim;

    void Start()
	{
        System.Diagnostics.Debug.WriteLine("Starting...");

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

        myText = GameObject.Find("Canvas2").GetComponent<Text>();

        GameObject ethan = GameObject.Find("Ethan2");
        c = (ThirdPersonCharacter)ethan.GetComponent(typeof(ThirdPersonCharacter));
        anim = (Animator)ethan.GetComponent(typeof(Animator));
    }

    float x, y, z = 0;
    Boolean hasUpdate = false;

    private void recv(IAsyncResult res)
    {
        //System.Diagnostics.Debug.WriteLine("received something");

        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
        byte[] receive_byte_array = listener.EndReceive(res, ref RemoteIpEndPoint);
        received_data = Encoding.UTF8.GetString(receive_byte_array);
        //System.Diagnostics.Debug.WriteLine(Encoding.UTF8.GetString(receive_byte_array));

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

        //System.Diagnostics.Debug.WriteLine("done translating, listening again...");

        hasUpdate = true;
        listener.BeginReceive(new AsyncCallback(recv), null);
    }

    private void Update()
    {
        if (hasUpdate)
        {
            float oldx, oldy, oldz;
            //System.Diagnostics.Debug.WriteLine("Translate from " + oldx + " " + oldy + " " + oldz);
            //System.Diagnostics.Debug.WriteLine("About to translate " + x + " " + y + " " + z);

            //Process codes

            oldx = this.gameObject.transform.position.x;
            oldy = this.gameObject.transform.position.y;
            oldz = this.gameObject.transform.position.z;

            Vector3 start = new Vector3(oldx, oldy, oldz);
            Vector3 end = new Vector3(x, y, z);
            var distanceTo = start - end;
            var smooth = 0.1f;

            // manual control
            transform.position = Vector3.Lerp(start, end, Time.deltaTime * smooth);

            // game play control
            //c.Move(end, false, false);

            // translates by an offset
            //transform.Translate(x, y, z, Space.World);

            // herky jerky motion
            //transform.position = end;

            Vector3 oldVec = new Vector3(oldx, oldy, oldz);
            Vector3 newVec = new Vector3(x, y, z);
            Vector3 direction = newVec - oldVec;
            //            Vector3 onlyY = new Vector3(0, direction.y, 0);

            Quaternion oldrot = transform.rotation;
            Quaternion newrot = Quaternion.LookRotation(direction, Vector3.up);

            // jerky way
            //             transform.rotation = newrot;
            float turnrate = 30;
            transform.rotation = Quaternion.RotateTowards(oldrot, newrot, turnrate * Time.deltaTime);

            // display x,y,z on canvas
            myText.text =
                x + Environment.NewLine +
                y + Environment.NewLine +
                z;

            hasUpdate = false;
        }
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        System.Diagnostics.Debug.WriteLine("selected");
        float oldx, oldy, oldz;
        oldx = this.gameObject.transform.position.x;
        oldy = this.gameObject.transform.position.y;
        oldz = this.gameObject.transform.position.z;

        // If the sphere has no Rigidbody component, add one to enable physics.
        //if (!this.GetComponent<Rigidbody>())
        //{
        //    var rigidbody = this.gameObject.AddComponent<Rigidbody>();
        //    rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //}
    }
}
