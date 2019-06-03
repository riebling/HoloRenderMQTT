using UnityEngine;
using UnityEngine.UI;
using System;
//using System.Net;
//using System.Net.Sockets;
using System.Text;
using UnityStandardAssets.Characters.ThirdPerson;
//using uPLibrary.Networking.M2Mqtt.Messages;

public class ObjectController : MonoBehaviour
{
    string received_data;
    Text myText;
    private Boolean startNewLine;

    // public vars appear in Unity Inspector window, thought it mangles their names
    // by inserting spaces e.g. "worldCursor" -> "World Cursor"

    // By setting these at compile time, saves compute
    // alternative technique is to set at Start() time
    public GameObject theCursor;
    public GameObject fireObject;
    public Boolean drawTrajectories = true;

    public float targetX, targetY, targetZ;
    public Boolean hasUpdate;
    private LineDrawer ld;

    void Start()
	{
        System.Diagnostics.Debug.WriteLine(this.name + " Starting ObjectController script...");

        // the coordinates display canvas attached to this GameObject
        myText = this.GetComponentInChildren<Text>();

        startNewLine = true;

        /* demonstrates how to access code from another object
        GameObject ethan = GameObject.Find("Ethan2");
        c = (ThirdPersonCharacter)ethan.GetComponent(typeof(ThirdPersonCharacter));
        anim = (Animator)ethan.GetComponent(typeof(Animator));
        */
    }

    // MQTT receive callback
    // this code runs when a message is received
    /*
        public void MqttReceiveCallback(object sender, MqttMsgPublishEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("received something");

            received_data = Encoding.UTF8.GetString(e.Message);
            //System.Diagnostics.Debug.WriteLine(Encoding.UTF8.GetString(receive_byte_array));

            // parse string
            string[] splits = received_data.Split(',');
            targetX = float.Parse(splits[0]);
            targetY = float.Parse(splits[1]);
            targetZ = float.Parse(splits[2]);

            hasUpdate = true;

            System.Diagnostics.Debug.WriteLine("MQTT: " + received_data);
        }
        */

    private void Update()
    {
        if (hasUpdate)
        {
            float oldx, oldy, oldz;
            //System.Diagnostics.Debug.WriteLine("hasUpdate TRUE");

            oldx = transform.position.x;
            oldy = transform.position.y;
            oldz = transform.position.z;

            // done moving. For some reason "==" is approximate :)
            if (oldx == targetX && oldy == targetY && oldz == targetZ)
            {
                //System.Diagnostics.Debug.WriteLine("STOPPED moving");
                hasUpdate = false;
                startNewLine = true;
                return;
            }

            // Move the GameObject but gradually
            Vector3 start = transform.position; //new Vector3(oldx, oldy, oldz);
            Vector3 end = new Vector3(targetX, targetY, targetZ);
            Vector3 distanceTo = end - start;
            float smooth = 1.0f; // heuristically determined smoothing rate; tweak as needed
            transform.position = Vector3.MoveTowards(start, end, Time.deltaTime * smooth);

            // Draw trajectory
            ld = new LineDrawer(0.01f);
            ld.DrawLineInGameView(new Vector3(oldx, oldy, oldz), transform.position, Color.magenta);
//            Debug.DrawLine(new Vector3(oldx, oldy, oldz), transform.position, Color.magenta, 10);

            // relative motion
            //transform.Translate(targetX, targetY, targetZ, Space.World);

            // herky jerky motion
            //transform.position = end;
            // transform.rotation = newrot; // jerky way

            // Rotate the GameObject but in a gradual way
            Quaternion oldrot = transform.rotation;
            Quaternion newrot = Quaternion.LookRotation(distanceTo, Vector3.up);
            float turnrate = 60; // heuristically determined smooth rotation rate, tweak as desired
            transform.rotation = Quaternion.RotateTowards(oldrot, newrot, turnrate * Time.deltaTime);

            // display x,y,z on attached canvas GameObject
            if (myText != null) myText.text =
                oldx + Environment.NewLine +
                oldy + Environment.NewLine +
                oldz;

            // hasUpdate = false;
        }
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        GetComponent<AudioSource>().Play();

        System.Diagnostics.Debug.WriteLine(this.name.ToString() + " About to instantiate " +
            theCursor.transform.position.x + "," +
            theCursor.transform.position.y + "," +
            theCursor.transform.position.z);

        GameObject o = Instantiate(fireObject, transform);
        o.SetActive(true); // Come on baby light my fire

        // Example code: how to enable physics.
        //if (!this.GetComponent<Rigidbody>())
        //{
        //    var rigidbody = this.gameObject.AddComponent<Rigidbody>();
        //    rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        //}
    }
}
