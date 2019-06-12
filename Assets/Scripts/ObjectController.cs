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

    // public vars appear in Unity Inspector window, thought it mangles their names
    // by inserting spaces e.g. "worldCursor" -> "World Cursor"

    // By setting these at compile time, saves compute
    // alternative technique is to set at Start() time
    public GameObject theCursor;
    public GameObject fireObject;
    public Boolean drawTrajectories = true;

    public float targetX, targetY, targetZ;
    public float targetXrot, targetYrot, targetZrot, targetWrot;
    public Boolean hasUpdate;
    private LineDrawer ld;

    void Start()
	{
        System.Diagnostics.Debug.WriteLine(this.name + " Starting ObjectController script...");

        // the coordinates display canvas attached to this GameObject
        myText = this.GetComponentInChildren<Text>();

        /* demonstrates how to access code from another object
        GameObject ethan = GameObject.Find("Ethan2");
        c = (ThirdPersonCharacter)ethan.GetComponent(typeof(ThirdPersonCharacter));
        anim = (Animator)ethan.GetComponent(typeof(Animator));
        */

        ld = new LineDrawer(0.01f);
    }

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
                return;
            }

            // Move the GameObject but gradually
            Vector3 start = transform.position;
            Vector3 end = new Vector3(targetX, targetY, targetZ);
            Vector3 distanceTo = end - start;

            float smooth = 1.0f; // heuristically determined smoothing rate; tweak as needed
            transform.position = Vector3.MoveTowards(start, end, Time.deltaTime * smooth);
            //transform.position = end; // herky jerky motion - no smoothing

            // Draw trajectory
            if (drawTrajectories)
            {
                ld.AddLine(new Vector3(oldx, oldy, oldz), transform.position, Color.magenta);
            }
            //debug.drawline looks better but only works in Unity editor
            //Debug.DrawLine(new Vector3(oldx, oldy, oldz), transform.position, Color.magenta, 10);

            // relative motion
            //transform.Translate(targetX, targetY, targetZ, Space.World);

            // transform.rotation = newrot;

            Quaternion oldrot = transform.rotation;
            Quaternion newrot = new Quaternion(targetXrot, targetYrot, targetZrot, targetWrot);
            float turnrate = 60; // heuristically determined smooth rotation rate, tweak as desired

            // Rotate the GameObject but in a gradual way
            transform.rotation = Quaternion.RotateTowards(oldrot, newrot, turnrate * Time.deltaTime);
            //transform.rotation = newrot; // this rotates immediately

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
