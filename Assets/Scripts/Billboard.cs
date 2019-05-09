// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    public enum PivotAxis
    {
        // Most common options, preserving current functionality with the same enum order.
        XY,
        Y,
        // Rotate about an individual axis.
        X,
        Z,
        // Rotate about a pair of axes.
        XZ,
        YZ,
        // Rotate about all axes.
        Free
    }

    /// <summary>
    /// The Billboard class implements the behaviors needed to keep a GameObject oriented towards the user.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Vector3 latePosition;
        private Quaternion lateRotation;

        /// <summary>
        /// The axis about which the object will rotate.
        /// </summary>
        [Tooltip("Specifies the axis about which the object will rotate.")]
        [SerializeField]
        private PivotAxis pivotAxis = PivotAxis.Y;
        public PivotAxis PivotAxis
        {
            get { return pivotAxis; }
            set { pivotAxis = value; }
        }

        /// <summary>
        /// The target we will orient to. If no target is specified, the main camera will be used.
        /// </summary>
        [Tooltip("Specifies the target we will orient to. If no target is specified, the main camera will be used.")]
        private Transform cameraTransform;
        public Transform CameraTransform
        {
            get { return cameraTransform; }
            set { cameraTransform = value; }
        }

        private void OnEnable()
        {
            if (CameraTransform == null)
            {
                CameraTransform = Camera.main.transform;
            }

            Update();
        }

        private void Start()
        {
            
        }
        private void LateUpdate()
        {
            transform.position = latePosition;
            transform.rotation = lateRotation;
        }

        /// <summary>
        /// Keeps the object facing the camera.
        /// </summary>
        private void Update()
        {
            if (CameraTransform == null)
            {
                return;
            }

            // Get a Vector that points from the target to the main camera.
            Vector3 directionToTarget = CameraTransform.position - transform.position;
            Vector3 targetUpVector = Camera.main.transform.up;

            // Get location 1m in front of camera

            Matrix4x4 m = Camera.main.cameraToWorldMatrix;
            Vector3 p = m.MultiplyPoint(new Vector3(0, 0, -1.0F));
            latePosition = p;

//            transform.position = new Vector3(CameraTransform.position.x + 0.5f,
//                CameraTransform.position.y - 0.5f,
//                CameraTransform.position.z + 1);

//            Text myText = GameObject.Find("Canvas").GetComponent<Text>();

            // Adjust for the pivot axis.
            switch (PivotAxis)
            {
                case PivotAxis.X:
                    directionToTarget.x = 0.0f;
                    targetUpVector = Vector3.up;
                    break;

                case PivotAxis.Y:
                    directionToTarget.y = 0.0f;
                    targetUpVector = Vector3.up;
                    break;

                case PivotAxis.Z:
                    directionToTarget.x = 0.0f;
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.XY:
                    targetUpVector = Vector3.up;
                    break;

                case PivotAxis.XZ:
                    directionToTarget.x = 0.0f;
                    break;

                case PivotAxis.YZ:
                    directionToTarget.y = 0.0f;
                    break;

                case PivotAxis.Free:
                default:
                    // No changes needed.
                    break;
            }

            // If we are right next to the camera the rotation is undefined. 
            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

//            myText.text = directionToTarget.x + Environment.NewLine +
//                directionToTarget.y + Environment.NewLine +
//                directionToTarget.z;

            // Calculate and apply the rotation required to reorient the object
            lateRotation = Quaternion.LookRotation(-directionToTarget, targetUpVector);
        }
    }
}
