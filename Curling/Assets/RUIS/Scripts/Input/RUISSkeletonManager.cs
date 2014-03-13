/*****************************************************************************

Content    :   A class to manage Kinect/OpenNI skeleton data
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/


using UnityEngine;
using System.Collections;

public class RUISSkeletonManager : MonoBehaviour {
    RUISCoordinateSystem coordinateSystem;

    public enum Joint
    {
        Root,
        Head,
        Torso,
        LeftShoulder,
        LeftElbow,
        LeftHand,
        RightShoulder,
        RightElbow,
        RightHand,
        LeftHip,
        LeftKnee,
        LeftFoot,
        RightHip,
        RightKnee,
        RightFoot,
        None
    }

    public class JointData
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float positionConfidence = 0.0f;
        public float rotationConfidence = 0.0f;
    }

    public class Skeleton
    {
        public bool isTracking = false;
        public JointData root = new JointData();
        public JointData head = new JointData();
        public JointData torso = new JointData();
        public JointData leftShoulder = new JointData();
        public JointData leftElbow = new JointData();
        public JointData leftHand = new JointData();
        public JointData rightShoulder = new JointData();
        public JointData rightElbow = new JointData();
        public JointData rightHand = new JointData();
        public JointData leftHip = new JointData();
        public JointData leftKnee = new JointData();
        public JointData leftFoot = new JointData();
        public JointData rightHip = new JointData();
        public JointData rightKnee = new JointData();
        public JointData rightFoot = new JointData();
    }

    NIPlayerManager playerManager;

    public readonly int skeletonsHardwareLimit = 4;

    public Skeleton[] skeletons = new Skeleton[4];

    

    public Vector3 rootSpeedScaling = Vector3.one;

    void Awake()
    {
        playerManager = GetComponent<NIPlayerManager>();
        

        if (coordinateSystem == null)
        {
            coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
        }

        for (int i = 0; i < skeletons.Length; i++)
        {
            skeletons[i] = new Skeleton();
        }
    }

    void Start()
    {
        
	}
	
	void Update () {
        for (int i = 0; i < playerManager.m_MaxNumberOfPlayers; i++)
        {
            skeletons[i].isTracking = playerManager.GetPlayer(i).Tracking;

            if (!skeletons[i].isTracking) continue;

            UpdateRootData(i);
            UpdateJointData(OpenNI.SkeletonJoint.Head, i, ref skeletons[i].head);
            UpdateJointData(OpenNI.SkeletonJoint.Torso, i, ref skeletons[i].torso);
            UpdateJointData(OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons[i].leftShoulder);
            UpdateJointData(OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons[i].leftElbow);
            UpdateJointData(OpenNI.SkeletonJoint.LeftHand, i, ref skeletons[i].leftHand);
            UpdateJointData(OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons[i].rightShoulder);
            UpdateJointData(OpenNI.SkeletonJoint.RightElbow, i, ref skeletons[i].rightElbow);
            UpdateJointData(OpenNI.SkeletonJoint.RightHand, i, ref skeletons[i].rightHand);
            UpdateJointData(OpenNI.SkeletonJoint.LeftHip, i, ref skeletons[i].leftHip);
            UpdateJointData(OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons[i].leftKnee);
            UpdateJointData(OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons[i].leftFoot);
            UpdateJointData(OpenNI.SkeletonJoint.RightHip, i, ref skeletons[i].rightHip);
            UpdateJointData(OpenNI.SkeletonJoint.RightKnee, i, ref skeletons[i].rightKnee);
            UpdateJointData(OpenNI.SkeletonJoint.RightFoot, i, ref skeletons[i].rightFoot);
        }
	}

    private void UpdateRootData(int player)
    {
        OpenNI.SkeletonJointTransformation data;

        if (!playerManager.GetPlayer(player).GetSkeletonJoint(OpenNI.SkeletonJoint.Torso, out data))
        {
            return;
        }

        Vector3 newRootPosition = coordinateSystem.ConvertKinectPosition(data.Position.Position);
        newRootPosition = Vector3.Scale(newRootPosition, rootSpeedScaling);
        skeletons[player].root.position = newRootPosition;
        skeletons[player].root.positionConfidence = data.Position.Confidence;
        skeletons[player].root.rotation = coordinateSystem.ConvertKinectRotation(data.Orientation);
        skeletons[player].root.rotationConfidence = data.Orientation.Confidence;
    }

    private void UpdateJointData(OpenNI.SkeletonJoint joint, int player, ref JointData jointData)
    {
        OpenNI.SkeletonJointTransformation data;

        if (!playerManager.GetPlayer(player).GetSkeletonJoint(joint, out data))
        {
            return;
        }

        jointData.position = coordinateSystem.ConvertKinectPosition(data.Position.Position);
        jointData.positionConfidence = data.Position.Confidence;
        jointData.rotation = coordinateSystem.ConvertKinectRotation(data.Orientation);
        jointData.rotationConfidence = data.Orientation.Confidence;
    }

    public JointData GetJointData(Joint joint, int player)
    {
        if (player >= playerManager.m_MaxNumberOfPlayers)
            return null;

        switch (joint)
        {
            case Joint.Root:
                return skeletons[player].root;
            case Joint.Head:
                return skeletons[player].head;
            case Joint.Torso:
                return skeletons[player].torso;
            case Joint.LeftShoulder:
                return skeletons[player].leftShoulder;
            case Joint.LeftElbow:
                return skeletons[player].leftElbow;
            case Joint.LeftHand:
                return skeletons[player].leftHand;
            case Joint.RightShoulder:
                return skeletons[player].rightShoulder;
            case Joint.RightElbow:
                return skeletons[player].rightElbow;
            case Joint.RightHand:
                return skeletons[player].rightHand;
            case Joint.LeftHip:
                return skeletons[player].leftHip;
            case Joint.LeftKnee:
                return skeletons[player].leftKnee;
            case Joint.LeftFoot:
                return skeletons[player].leftFoot;
            case Joint.RightHip:
                return skeletons[player].rightHip;
            case Joint.RightKnee:
                return skeletons[player].rightKnee;
            case Joint.RightFoot:
                return skeletons[player].rightFoot;
            default:
                return null;
        }
    }
}
