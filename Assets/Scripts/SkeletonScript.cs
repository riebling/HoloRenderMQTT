using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OpenPose3dSkeletonFactory;

public class SkeletonScript : MonoBehaviour
{
    public Boolean hasUpdate;
    /*
    public Vector3 Nose, Neck, RShoulder, RElbow, RWrist, LShoulder, LElbow, LWrist, MidHip,
 RHip, RKnee, RAnkle, LHip, LKnee, LAnkle,
 REye, LEye, REar, LEar, LBigToe, LSmallToe,
 LHeel, RBigToe, RSmallToe, RHeel, Background;
    */

    public SkeletonData sd;

    private LineRenderer leftHead;
    private LineRenderer rightHead;
    private LineRenderer torso;
    private LineRenderer leftArm;
    private LineRenderer rightArm;
    private LineRenderer leftLeg;
    private LineRenderer rightLeg;
    private LineRenderer leftFoot;
    private LineRenderer rightFoot; 

    // Start is called before the first frame update
    void Start()
    {
        leftHead = (LineRenderer)transform.Find("leftHead").GetComponent<LineRenderer>();
        rightHead = (LineRenderer)transform.Find("rightHead").GetComponent<LineRenderer>();
        torso = (LineRenderer)transform.Find("torso").GetComponent<LineRenderer>();
        leftArm = (LineRenderer)transform.Find("leftArm").GetComponent<LineRenderer>();
        rightArm = (LineRenderer)transform.Find("rightArm").GetComponent<LineRenderer>();
        leftLeg = (LineRenderer)transform.Find("leftLeg").GetComponent<LineRenderer>();
        rightLeg = (LineRenderer)transform.Find("rightLeg").GetComponent<LineRenderer>();
        leftFoot = (LineRenderer)transform.Find("leftFoot").GetComponent<LineRenderer>();
        rightFoot = (LineRenderer)transform.Find("rightFoot").GetComponent<LineRenderer>();
    }

    void tryDraw(LineRenderer lr, Vector3[] positions)
    {
        lr.gameObject.SetActive(true);

        for (int i = positions.Length - 1; i >= 0; i--)
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
        if (hasUpdate)
        {
            tryDraw(leftHead, new[] { sd.Nose, sd.LEye, sd.LEar });
            tryDraw(rightHead, new[] { sd.Nose, sd.REye, sd.REar });
            tryDraw(torso, new[] { sd.Nose, sd.Neck, sd.MidHip });
            tryDraw(leftArm, new[] { sd.Neck, sd.LShoulder, sd.LElbow, sd.LWrist });
            tryDraw(rightArm, new[] { sd.Neck, sd.RShoulder, sd.RElbow, sd.RWrist });
            tryDraw(leftLeg, new[] { sd.MidHip, sd.LHip, sd.LKnee, sd.LAnkle });
            tryDraw(rightLeg, new[] { sd.MidHip, sd.RHip, sd.RKnee, sd.RAnkle });
            tryDraw(leftFoot, new[] { sd.LHeel, sd.LAnkle, sd.LBigToe, sd.LSmallToe });
            tryDraw(rightFoot, new[] { sd.RHeel, sd.RAnkle, sd.RBigToe, sd.RSmallToe });
            hasUpdate = false;
        }
    }
}
