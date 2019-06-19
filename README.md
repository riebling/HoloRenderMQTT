# HoloRenderMQTT README

## setup 

The unity 'scene' might be named 'x' and not HoloRenderMQTT like this repository
There might also be artifacts of the previous folder/project name 'Origami'

When importing into Unity, you should have a couple prerequisites set up:

 * Visual Studio support for UWP (Universal Windows Platform) - HoloLens, even if you don't have the hardware, some code will want certain libraries. To install, run Visual Studio Installer, choose Modify, check the checkbox for "Universal Windows Platform development" under Workloads
 * SpatialMapping - this is HoloLens' ability to make & use 3d scans of the space it's in, create a 'room mesh' and collider for it. There are some Unity glitches that, when first importing projects with SpatialMapping, don't find components properly. Telltale sign: `The type or namespace SpatialMappingRenderer could not be found` (or SpatialMappingCollider) error in Unity's Console window.
 The work-around is to go to File->Build Settings->Player Settings...->Player->(tab for Universal Windows Platform settings)->XR Settings, uncheck, then re-check the "Virtual Reality Supported" checkbox. Even upon doing this, not all the SpatialMapping references were resolved; closing then re-opening the Unity project seems to fully resolve them, at least for me testing the release.
 * These should be included in the Unity project, but to be sure, in File->Build Settings->Player Settings...->Player->(tab for Universal Windows Platform settings)->Publishing Settings under the Capabilities heading, these should be checked:
   - InternetClient
   - InternetClientServer
   - PrivateNetworkClientServer
   - SharedUserCertificates
   - Microphone
   - Location
   - SpatialPerception
   - RemoteSystem  
Maybe not all of them are required, we can ablation test to find out the minimum required capabilities, but including these can't hurt.
 * The same goes for all the files in `Assets/` - Unity has this habit of dumping more than you want when importing things from the Asset Store into the project assets, even when you try to pick and choose only what you want, often leaving out some dependent other piece like a prefab, script or texture. I'm not sure the best practice for keeping project size small, Unity seems to prefer to import massive resources into Assets (and re-import duplicate assets across multiple projects) and only strip away unused ones when you Build and, eventually, deploy.
 
## MQTT
Part of the beauty of MQTT is heterogeneous devices do not need to know each others' IP address or hostname, only the the hostname of the MQTT broker. It's deceptively simple to set up a MQTT broker on linux:
```
sudo apt-get install mosquitto mosquitto-clients
```
Likewise it's deceptively simple to use it:
```
mosquitto-pub -h <hostname> -t <topic> -m <message>
mosquitto-sub -h <hostname> -t <topic>
```
We have an MQTT broker set up at `oz.andrew.cmu.edu` and the HoloRenderMQTT app uses the `/topic/render`; it is intended that objects wishing to report their position for rendering publish to this topic simple messages like, for now:
```
drone,-1,2,1,0.02,0.01,0,-0.02,on
```
Meaning: display the drone at coordinates -1,2,1 meters, relative to the headset (camera in Unity) frame of reference. Rotation is oriented at .02,.01,0,-.02 as an x,y,z,w quaternion.
The headset coordinate system is established as 0,0,0 when first running the program.

There is another topic for the headset to report it's own coordinates: `/topic/headset` and a few other topics for our UWB trackers that uses a fancier format, found in the `Mqtt.cs` code (more on this later).

The simplest way to send test data is with single MQTT publish commands, for example:
```
mosquitto_pub -h oz.andrew.cmu.edu -t /topic/render
```
## Sending fake MQTT to HoloRenderMQTT Unity app
 Example code to send fake MQTT position data is in `mover.py` which takes 1 argument: a name of a 'holo object' to draw, one of 'drone', 'avatar', 'fire', 'line'. (It should probably be modified to take the name of the MQTT broker, which is currently hard coded to `oz.andrew.cmu.edu` which wil actually work, but we probably want separate, local ones) There are other parameters in mover.py for update rate (default 10fps), how much motion occurs per update (0.1 meter). This is just to test communication - "may not resemble real data"
 
## Unity Project Structure
Since Unity is so literally object oriented, with code attached to objects, a breakdown of what's in the scene:
### Main Camera
self explanatory; has headset code attached `HeadsetController.cs`
### Lights
self explanatory, no code attached. It'd be neat if Holo objects were all self illuminating e.g. "full bright" setting in Second Life, since shadows from world lights might render stuff darkly when not intended
### Terrain
Not really used, this is more for VR where you have a ground plane that physical objects (Unity "rigidbody" turned on, meaning physics applies to them) don't pass through
### Cursor
This is for fun, an artifact of the Origami tutorial; a donut shaped cursor in the direction of headset gaze, will track surfaces of 3d virtual objects or room mesh Spatial Mapping - coupled with `WorldCursor.cs` script and the World object (described later) can give us the location of something when a control gesture is detected. The fun part: set the object or surface on fire when clicked
### Avatar
(Avatar, Drone, Fire, Line objects are similar) - a parent object that has as it's children the default Unity3d avatar ("Ethan"), a canvas to display coordinates, and importantly, the `ObjectController.cs` script. Position information updates the parent, thus updating the child (whatever flavor object it might be), moves the canvas along with it. A setting 'Draw Trajectories' in it's Object Controller script (described later) controls rendering of line segments as the object is moved
#### Canvas
Canvas object for drawing text. (other objects have similar Canvas children) Used to draw coordinates 'next to' 3d objects
### Drone
Similar to Avatar. A 3d model of a Crazyflie drone, with attached canvas for coordinate display.
### Skeleton
A stick figure skeleton made up of colored lines connecting various dots: leftHead, rightHead, torso, leftArm, rightArm, leftLag, rightLeg, leftFoot, rightFoot. Renders skeleton coordinate data from OpenPose. This object gets cloned once for each body detected by OpenPose by SkeletonFactory (see below)
### World
This handles things like the room mesh for Spatial mapping, and other top level scripts like
 * `ObjectFactory.cs` - interprets requests to instantiate and delete the various (Avatar, Drone, etc.) objects. Lists available movable objects.
 * `GazeGesturemanager.cs` - handles the airTap 'clicked' gesture. Other controllers (Vive) code should reside here.
 * `WallCommands.cs` - what happens if the room mesh Spatial Mapping surface is clicked? Fire, of course!
#### SpatialMapping
child object which is somewhat HoloLens specific, for tracking the detected room mesh. If SpatialMappingCollider is enabled, 3d objects with colliders cannot pass through, just like real objects cannot. SpatialMappingRenderer, if enabled, displays the 3d room mesh
### myFire
Fire object, can be given same movement & on/off commands as Avatar,Drone class since it contains `ObjectController.cs` script. Currently uses standard Unity asset 'FireMobile' which is more lightweight, though 'FireComplex' can be substituted for more spectacular effect
### Line
similar to Avatar and drone, but without anything 3d; just a trajectory. Turns out lines are very complicated in Unity. When enabled via the Draw Trajectories checkbox, instantiates a 3d Line object for each movement update. Should probably die off after a while rather than remain onscreen and "in world" using up memory resources. Maybe an enhancement to turn off lines will remove all the lines previously drawn. It's a shame the simple, straightforward Debug.DrawLine primitive only works in Unity player, would love this functionality to play on HoloLens, but nope, "for debugging only.

## Scripts
### `ObjectController.cs`
The most detailed script. Contains logic to smoothly move an object every frame towards a TargetX,TargetY,TargetZ cordinate, and target Rotx,Roty,Rotz,RotW quaternion orientation. Knows if it needs to move based on the HasUpdate flag. Knows about other GameObjects The Cursor and Fire Object so that anything with ObjectController.cs script attached can be set aflame, and fire remains attached & moves with. Has a parameter to turn on or off the drawing of the trajectory taken by the GameObject it is attached to (Avatar, Drone, etc)
### `Mqtt.cs`
A standalone class, this demonstrates MQTT callbacks, for storing movement updates in a queue which gets emptied out by various movable GameObjects. The `Send` method is for publishing headset coordinates. Has a sample callback function - the actual one is in `ObjectLibrary.cs` which parses messages and maps strings like 'drone', 'avatar' to actual Unity3d GameObjects. For now objects dequeue their movement updates every frame, so no movements are lost. But we may want to "fast forward" to only the most recent coordinate for performance reasons. Some settings are hard coded in here (that should be promoted to Public values settable in Unity3d editor's Inspector window):
```
    // string BrokerAddress = "johnpi.local";
    public string BrokerAddress = "oz.andrew.cmu.edu";

    // Subscribe topics
    public string tracker1Topic = "/topic/uwb_range_1";
    public string tracker2Topic = "/topic/uwb_range_2";
    public string renderTopic = "/topic/render";
    public string skeletonTopic = "/topic/skeleton";

    // Publish topics - our coordinates + orientation

    //    public string headsetTopic = "/topic/vio_1"; // this is what John's demo uses
    public string headsetTopic = "/topic/hololens"; // this is what John's demo uses
```
### SkeletonFactory.cs
Has it's own MQTT topic `/topic/skeleton` since the message format is different. Interprets coordinates from OpenPose as 24 sets of x,y,z for each body joint tracked, and handles multiple person tracking by cloning multiple skeleton GameObjects.
