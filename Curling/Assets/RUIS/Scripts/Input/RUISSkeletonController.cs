/*****************************************************************************

Content    :   Functionality to control a skeleton using Kinect
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSkeletonController")]
public class RUISSkeletonController : MonoBehaviour
{
    public Transform root;
    public Transform head;
    public Transform neck;
    public Transform torso;
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightHand;
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightFoot;
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftHand;
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftFoot;
	
	private RUISInputManager inputManager;
    private RUISSkeletonManager skeletonManager;
	private RUISCharacterController characterController;

    public int playerId = 0;

    private Vector3 skeletonPosition = Vector3.zero;

    public bool updateRootPosition = true;
    public bool updateJointPositions = true;
    public bool updateJointRotations = true;

    public bool useHierarchicalModel = false;
    public bool scaleHierarchicalModelBones = true;
    public float maxScaleFactor = 0.01f;

    public float minimumConfidenceToUpdate = 0.5f;

    public float rotationDamping = 15f;
	
	public bool followMoveController { get; private set; }
	private int followMoveID = 0;
	private RUISPSMoveWand psmove;
	
	private KalmanFilter positionKalman;
	private double[] measuredPos = {0, 0, 0};
	private double[] pos = {0, 0, 0};
	private float positionNoiseCovariance = 500;
	
    private Dictionary<Transform, Quaternion> jointInitialRotations;
    private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;

    [HideInInspector]
    public float torsoOffset = 0.0f;
    [HideInInspector]
    public float torsoScale = 1.0f;

    public float neckHeightTweaker = 0.0f;
    private Vector3 neckOriginalLocalPosition;

    public float forearmLengthRatio = 1.0f;
    private Vector3 originalRightForearmScale;
    private Vector3 originalLeftForearmScale;
	
	public float shinLengthRatio = 1.0f;
	private Vector3 originalRightShinScale;
	private Vector3 originalLeftShinScale;

    void Awake()
    {
        if (skeletonManager == null)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }
		
		followMoveController = false;
		
        jointInitialRotations = new Dictionary<Transform, Quaternion>();
        jointInitialDistances = new Dictionary<KeyValuePair<Transform, Transform>, float>();
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3,3);
    }

    void Start()
    {
        if (useHierarchicalModel)
        {
            //fix all shoulder and hip rotations to match the default kinect rotations
            rightShoulder.rotation = FindFixingRotation(rightShoulder.position, rightElbow.position, transform.right) * rightShoulder.rotation;
            leftShoulder.rotation = FindFixingRotation(leftShoulder.position, leftElbow.position, -transform.right) * leftShoulder.rotation;
            rightHip.rotation = FindFixingRotation(rightHip.position, rightFoot.position, -transform.up) * rightHip.rotation;
            leftHip.rotation = FindFixingRotation(leftHip.position, leftFoot.position, -transform.up) * leftHip.rotation;

            Vector3 assumedRootPos = (rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4;
            Vector3 realRootPos = torso.position;
            torsoOffset = (realRootPos - assumedRootPos).y;

            if (neck)
            {
                neckOriginalLocalPosition = neck.localPosition;
            }
        }

        SaveInitialRotation(root);
        SaveInitialRotation(head);
        SaveInitialRotation(torso);
        SaveInitialRotation(rightShoulder);
        SaveInitialRotation(rightElbow);
        SaveInitialRotation(rightHand);
        SaveInitialRotation(leftShoulder);
        SaveInitialRotation(leftElbow);
        SaveInitialRotation(leftHand);
        SaveInitialRotation(rightHip);
        SaveInitialRotation(rightKnee);
        SaveInitialRotation(rightFoot);
        SaveInitialRotation(leftHip);
        SaveInitialRotation(leftKnee);
        SaveInitialRotation(leftFoot);

        SaveInitialDistance(rightShoulder, rightElbow);
        SaveInitialDistance(rightElbow, rightHand);
        SaveInitialDistance(leftShoulder, leftElbow);
        SaveInitialDistance(leftElbow, leftHand);

        SaveInitialDistance(rightHip, rightKnee);
        SaveInitialDistance(rightKnee, rightFoot);
        SaveInitialDistance(leftHip, leftKnee);
        SaveInitialDistance(leftKnee, leftFoot);

        SaveInitialDistance(torso, head);

        SaveInitialDistance(rightShoulder, leftShoulder);
        SaveInitialDistance(rightHip, leftHip);

        if (rightElbow)
        {
            originalRightForearmScale = rightElbow.localScale;
        }

        if (leftElbow)
        {
            originalLeftForearmScale = leftElbow.localScale;
        }

		if(rightKnee)
		{
			originalRightShinScale = rightKnee.localScale;
		}
		
		if(leftKnee)
		{
			originalLeftShinScale = leftKnee.localScale;
		}

		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		if(inputManager && !inputManager.enableKinect)
		{
//			Debug.Log("Kinect is not enabled. Disabling RUISSkeletonController script from Kinect-controlled GameObject " + gameObject.name + ".");
//			MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
//			foreach(MonoBehaviour script in scripts)
//			{
//				if(script.GetType().Equals(typeof(RUISSkeletonController)))
//					script.enabled = false;
//			}
			
			if(gameObject.transform.parent != null)
			{
				characterController = gameObject.transform.parent.GetComponent<RUISCharacterController>();
				if(characterController != null)
					if(		characterController.characterPivotType == RUISCharacterController.CharacterPivotType.MoveController
						&&	inputManager.enablePSMove																			)
					{
						followMoveController = true;
						followMoveID = characterController.moveControllerId;
						if(		 gameObject.GetComponent<RUISKinectAndMecanimCombiner>() == null 
							||	!gameObject.GetComponent<RUISKinectAndMecanimCombiner>().enabled )
							Debug.LogWarning(	"Using PS Move controller #" + characterController.moveControllerId + " as a source "
											 +	"for avatar root position of " + gameObject.name + ", because Kinect is disabled "
											 +	"and PS Move is enabled, while that PS Move controller has been assigned as a "
											 +	"Character Pivot in " + gameObject.name + "'s parent GameObject");
					}
			}
		}
    }




    void LateUpdate()
    {
        if (skeletonManager != null && skeletonManager.skeletons[playerId] != null && skeletonManager.skeletons[playerId].isTracking)
        {
            UpdateSkeletonPosition();

            UpdateTransform(ref head, skeletonManager.skeletons[playerId].head);
            UpdateTransform(ref torso, skeletonManager.skeletons[playerId].torso);
            UpdateTransform(ref leftShoulder, skeletonManager.skeletons[playerId].leftShoulder);
            UpdateTransform(ref leftElbow, skeletonManager.skeletons[playerId].leftElbow);
            UpdateTransform(ref leftHand, skeletonManager.skeletons[playerId].leftHand);
            UpdateTransform(ref rightShoulder, skeletonManager.skeletons[playerId].rightShoulder);
            UpdateTransform(ref rightElbow, skeletonManager.skeletons[playerId].rightElbow);
            UpdateTransform(ref rightHand, skeletonManager.skeletons[playerId].rightHand);
            UpdateTransform(ref leftHip, skeletonManager.skeletons[playerId].leftHip);
            UpdateTransform(ref leftKnee, skeletonManager.skeletons[playerId].leftKnee);
            UpdateTransform(ref leftFoot, skeletonManager.skeletons[playerId].leftFoot);
            UpdateTransform(ref rightHip, skeletonManager.skeletons[playerId].rightHip);
            UpdateTransform(ref rightKnee, skeletonManager.skeletons[playerId].rightKnee);
            UpdateTransform(ref rightFoot, skeletonManager.skeletons[playerId].rightFoot);

            if (!useHierarchicalModel)
            {
                if (leftHand != null)
                {
                    leftHand.localRotation = leftElbow.localRotation;
                }

                if (rightHand != null)
                {
                    rightHand.localRotation = rightElbow.localRotation;
                }
            }
            else
            {
                if (scaleHierarchicalModelBones)
                {
                    UpdateBoneScalings();

                    Vector3 torsoDirection = skeletonManager.skeletons[playerId].torso.rotation * Vector3.down;
                    torso.position = transform.TransformPoint(skeletonManager.skeletons[playerId].torso.position - skeletonPosition - torsoDirection * torsoOffset * torsoScale);

                    ForceUpdatePosition(ref rightShoulder, skeletonManager.skeletons[playerId].rightShoulder);
                    ForceUpdatePosition(ref leftShoulder, skeletonManager.skeletons[playerId].leftShoulder);
                    ForceUpdatePosition(ref rightHip, skeletonManager.skeletons[playerId].rightHip);
                    ForceUpdatePosition(ref leftHip, skeletonManager.skeletons[playerId].leftHip);
                }
            }

            if (updateRootPosition)
            {
                Vector3 newRootPosition = skeletonManager.skeletons[playerId].root.position;
				
				measuredPos[0] = newRootPosition.x;
				measuredPos[1] = newRootPosition.y;
				measuredPos[2] = newRootPosition.z;
				positionKalman.setR(Time.deltaTime * positionNoiseCovariance);
			    positionKalman.predict();
			    positionKalman.update(measuredPos);
				pos = positionKalman.getState();
				
                transform.localPosition = new Vector3((float) pos[0], (float) pos[1], (float) pos[2]); //newRootPosition;
            }

        }
		else // TUUKKA
			if(followMoveController && characterController && inputManager)
			{
				psmove = inputManager.GetMoveWand(followMoveID);
				if(psmove)
				{
				
					Quaternion moveYaw = Quaternion.Euler(0, psmove.localRotation.eulerAngles.y, 0);
				
					skeletonPosition = psmove.localPosition - moveYaw*characterController.psmoveOffset;
					skeletonPosition.y = 0;
					
					if (updateRootPosition)
						transform.localPosition = skeletonPosition;
					
		            UpdateTransformWithPSMove(ref head, moveYaw);
		            UpdateTransformWithPSMove(ref torso, moveYaw);
		            UpdateTransformWithPSMove(ref leftShoulder, moveYaw);
		            UpdateTransformWithPSMove(ref leftElbow, moveYaw);
		            UpdateTransformWithPSMove(ref leftHand, moveYaw);
		            UpdateTransformWithPSMove(ref rightShoulder, moveYaw);
		            UpdateTransformWithPSMove(ref rightElbow, moveYaw);
		            UpdateTransformWithPSMove(ref rightHand, moveYaw);
		            UpdateTransformWithPSMove(ref leftHip, moveYaw);
		            UpdateTransformWithPSMove(ref leftKnee, moveYaw);
		            UpdateTransformWithPSMove(ref leftFoot, moveYaw);
		            UpdateTransformWithPSMove(ref rightHip, moveYaw);
		            UpdateTransformWithPSMove(ref rightKnee, moveYaw);
		            UpdateTransformWithPSMove(ref rightFoot, moveYaw);
				}
			}


        TweakNeckHeight();
    }

    private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        if (updateJointPositions && jointToGet.positionConfidence >= minimumConfidenceToUpdate)
        {
            transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
        }

        if (updateJointRotations && jointToGet.rotationConfidence >= minimumConfidenceToUpdate)
        {
            if (useHierarchicalModel)
            {
                Quaternion newRotation = transform.rotation * jointToGet.rotation *
                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
                transformToUpdate.rotation = Quaternion.RotateTowards(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDamping);
            }
            else
            {
                transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDamping);
            }
        }
    }
	
	private void UpdateTransformWithPSMove(ref Transform transformToUpdate, Quaternion moveYaw)
    {
		if (transformToUpdate == null) return;
		
		//if (updateJointPositions) ;
		
		if (updateJointRotations)
		{
			if (useHierarchicalModel)
            {
//                Quaternion newRotation = transform.rotation * jointToGet.rotation *
//                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
//                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDamping);
                Quaternion newRotation = transform.rotation * moveYaw *
                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
                transformToUpdate.rotation = newRotation;
            }
            else
            {
				transformToUpdate.localRotation = moveYaw;
//                transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDamping);
            }
		}
	}

    private void ForceUpdatePosition(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);//transform.position + transform.rotation * (jointToGet.position - skeletonPosition);
    }

    //gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
    private void UpdateSkeletonPosition()
    {
        if (skeletonManager.skeletons[playerId].root.positionConfidence >= minimumConfidenceToUpdate)
        {
            skeletonPosition = skeletonManager.skeletons[playerId].root.position;
        }
    }

    private void SaveInitialRotation(Transform bodyPart)
    {
        if (bodyPart)
            jointInitialRotations[bodyPart] = GetInitialRotation(bodyPart);
    }

    private void SaveInitialDistance(Transform rootTransform, Transform distanceTo)
    {
        jointInitialDistances.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), Vector3.Distance(rootTransform.position, distanceTo.position));
    }

    private Quaternion GetInitialRotation(Transform bodyPart)
    {
        return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
    }

    private void UpdateBoneScalings()
    {
        if (!ConfidenceGoodEnoughForScaling()) return;

        torsoScale = UpdateTorsoScale();

        {
            rightElbow.transform.localScale = originalRightForearmScale;
            float rightArmCumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeletonManager.skeletons[playerId].rightShoulder, skeletonManager.skeletons[playerId].rightElbow, torsoScale);
            UpdateBoneScaling(rightElbow, rightHand, skeletonManager.skeletons[playerId].rightElbow, skeletonManager.skeletons[playerId].rightHand, rightArmCumulativeScale);
            rightElbow.transform.localScale = rightElbow.transform.localScale * forearmLengthRatio;
        }

        {
            leftElbow.transform.localScale = originalLeftForearmScale;
            float leftArmCumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeletonManager.skeletons[playerId].leftShoulder, skeletonManager.skeletons[playerId].leftElbow, torsoScale);
            UpdateBoneScaling(leftElbow, leftHand, skeletonManager.skeletons[playerId].leftElbow, skeletonManager.skeletons[playerId].leftHand, leftArmCumulativeScale);
            leftElbow.transform.localScale = leftElbow.transform.localScale * forearmLengthRatio;
        }

        {
			rightKnee.transform.localScale = originalRightShinScale;
            float rightLegCumulativeScale = UpdateBoneScaling(rightHip, rightKnee, skeletonManager.skeletons[playerId].rightHip, skeletonManager.skeletons[playerId].rightKnee, torsoScale);
            UpdateBoneScaling(rightKnee, rightFoot, skeletonManager.skeletons[playerId].rightKnee, skeletonManager.skeletons[playerId].rightFoot, rightLegCumulativeScale);
			rightKnee.transform.localScale = rightKnee.transform.localScale * shinLengthRatio;
        }

        {
			leftKnee.transform.localScale = originalLeftShinScale;
            float leftLegCumulativeScale = UpdateBoneScaling(leftHip, leftKnee, skeletonManager.skeletons[playerId].leftHip, skeletonManager.skeletons[playerId].leftKnee, torsoScale);
			UpdateBoneScaling(leftKnee, leftFoot, skeletonManager.skeletons[playerId].leftKnee, skeletonManager.skeletons[playerId].leftFoot, leftLegCumulativeScale);
			leftKnee.transform.localScale = leftKnee.transform.localScale * shinLengthRatio;
        }
    }

    private float UpdateBoneScaling(Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, RUISSkeletonManager.JointData comparisonBoneTracker, float cumulativeScale)
    {
        float modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];
        float playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
        float newScale = playerBoneLength / modelBoneLength / cumulativeScale;

        boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, new Vector3(newScale, newScale, newScale), maxScaleFactor * Time.deltaTime);

        return boneToScale.localScale.x;
    }

    private float UpdateTorsoScale()
    {
        //average hip to shoulder length and compare it to the one found in the model - scale accordingly
        //we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
        float modelLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, leftHip)] +
                            jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2;
        float playerLength = (Vector3.Distance(skeletonManager.skeletons[playerId].rightShoulder.position, skeletonManager.skeletons[playerId].leftShoulder.position) +
                                Vector3.Distance(skeletonManager.skeletons[playerId].rightHip.position, skeletonManager.skeletons[playerId].leftHip.position)) / 2;

        float newScale = playerLength / modelLength;
        torso.localScale = new Vector3(newScale, newScale, newScale);
        return newScale;
    }

    private Quaternion FindFixingRotation(Vector3 fromJoint, Vector3 toJoint, Vector3 wantedDirection)
    {
        Vector3 boneVector = toJoint - fromJoint;
        return Quaternion.FromToRotation(boneVector, wantedDirection);
    }

    private void TweakNeckHeight()
    {
        if (!neck) return;
        neck.localPosition = neckOriginalLocalPosition - neck.InverseTransformDirection(Vector3.up) * neckHeightTweaker;
    }

    public bool ConfidenceGoodEnoughForScaling()
    {
        return !(skeletonManager.skeletons[playerId].rightShoulder.positionConfidence < minimumConfidenceToUpdate ||
               skeletonManager.skeletons[playerId].leftShoulder.positionConfidence < minimumConfidenceToUpdate ||
               skeletonManager.skeletons[playerId].rightHip.positionConfidence < minimumConfidenceToUpdate ||
               skeletonManager.skeletons[playerId].leftHip.positionConfidence < minimumConfidenceToUpdate);
    }
}
