using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityStandardAssets.Characters.ThirdPerson;
using uPLibrary.Networking.M2Mqtt.Messages;

public class WallCommands : MonoBehaviour
{
    // these appear in Unity Inspector window, thought it mangles their names
    // by inserting spaces e.g. "worldCursor" -> "World Cursor"
    public GameObject theCursor;
    public GameObject fireObject;

    void Start()
	{
    }

    private void Update()
    {
    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        System.Diagnostics.Debug.WriteLine(this.name.ToString() + " selected");

        System.Diagnostics.Debug.WriteLine("About to instantiate " +
            theCursor.transform.position.x + "," +
            theCursor.transform.position.y + "," +
            theCursor.transform.position.z);
        GameObject i = Instantiate(fireObject, theCursor.transform.position, theCursor.transform.rotation);

        i.SetActive(true);
    }
}
