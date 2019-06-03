#define UNITY_WSA_10_0
using UnityEngine;

// Support different target platforms; UNITY_WSA_10_0 = HoloLens
#if UNITY_WSA_10_0
using UnityEngine.XR.WSA.Input;
#endif

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }

#if UNITY_WSA_10_0
    GestureRecognizer recognizer;
#endif

    // Use this for initialization
    void Start()
    {
        Instance = this;
#if UNITY_WSA_10_0
        // Set up a GestureRecognizer to detect Tapped gestures.
        recognizer = new GestureRecognizer();
        recognizer.Tapped += (args) =>
        {
            System.Diagnostics.Debug.WriteLine("Tapped.");

            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null)
            {
                System.Diagnostics.Debug.WriteLine("FocusedObject: " +FocusedObject.name.ToString());

                FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
            }
        };
        recognizer.StartCapturingGestures();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            if (FocusedObject != null )System.Diagnostics.Debug.WriteLine("Focused:" + FocusedObject.name.ToString());
#if UNITY_WSA_10_0
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
#endif
        }
    }
}
