/*****************************************************************************

Content    :   Class for correcting sensor's yaw drift with Kinect or PS Move
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class YawDriftCorrector : MonoBehaviour {

	public enum CompassSource
	{
	    Kinect = 0,
	    PSMove = 1,
		InputTransform = 2
	};
	
	public enum DriftingRotation
	{
	    OculusRift = 0,
		RazerHydra = 1,
	    InputTransform = 2
	};
	
	public DriftingRotation driftingSensor = DriftingRotation.OculusRift;
	
	public int oculusID = 0; //
	OVRCameraController oculusCamController; //
	
	public Transform driftingTransform;
	
	
	public CompassSource compass = CompassSource.PSMove;
	
	public int kinectPlayerID = 0; //
	private RUISSkeletonManager skeletonManager; //
	public RUISSkeletonManager.Joint compassJoint = RUISSkeletonManager.Joint.Torso;
	public bool correctOnlyWhenFacingForward = true;
	private RUISSkeletonManager.JointData compassData;
	
	public int PSMoveID = 0; //
	private RUISPSMoveWand compassMove;
	
	public Transform compassTransform;
	
	public float driftCorrectionRate = 0.1f;
	
	RUISInputManager inputManager; //
	
	private Quaternion driftingRot = new Quaternion(0, 0, 0, 1);
	private Quaternion rotationDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion filteredYawDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion finalYawDifference = new Quaternion(0, 0, 0, 1);
	
	private Vector3 driftingEuler;
	private Vector3 compassEuler;
	private Vector3 yawDifferenceDirection;
	private Quaternion driftingYaw;
	private Quaternion compassYaw;
	
	private KalmanFilter filterDrift;
	
	private double[] measuredDrift = {0, 0};
	
	private double[] filteredDrift = {0, 0};
	
	public bool filterInFixedUpdate = false;
	
	public float driftNoiseCovariance = 3000;
	
	public GameObject driftingDirectionVisualizer;
	public GameObject compassDirectionVisualizer;
	public GameObject correctedDirectionVisualizer;
	public Transform driftVisualizerPosition;
	
    public void Awake()
    {
		filterDrift = new KalmanFilter();
		filterDrift.initialize(2,2);
		
	}
	
	
    public void Start()
    {
		switch(compass) 
		{
			case CompassSource.Kinect:
		        if (!skeletonManager)
		        {
		            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		        }
				if(!skeletonManager)
					Debug.LogError("RUISSkeletonManager script is missing from this scene!");
				break;
			case CompassSource.PSMove:
				inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
				if(!inputManager)
					Debug.LogError("RUISInputManager script is missing from this scene!");
				break;
		}
		
		if(driftingSensor == DriftingRotation.InputTransform && !driftingTransform)
			Debug.LogError("driftingTransform is none, you need to set it from the inspector!");
		
		oculusCamController = FindObjectOfType(typeof(OVRCameraController)) as OVRCameraController;
	}
	
	// Update is called once per frame
	void Update() 
	{
		/*
		// Frame rate slow down simulation :-)
		int iter = Random.Range(100000, 100000000);
		int result = 0;
		for(int i=0; i<iter; ++i)
		{
			result = (iter + 980179)%(iter + 123020211);
		}
		*/
		
		if(!filterInFixedUpdate) 
		{
			doYawFiltering(Time.deltaTime);
		}
	}
	
	void FixedUpdate() 
	{
		if(filterInFixedUpdate) 
		{
			doYawFiltering(Time.fixedDeltaTime);
		}
	}
	
	private void doYawFiltering(float deltaT)
	{
			
		switch(driftingSensor) 
		{
			case DriftingRotation.OculusRift:
				if(OVRDevice.IsSensorPresent(oculusID))
				{
					OVRDevice.GetOrientation(oculusID, ref driftingRot);
					if(oculusCamController)
					{
						// In the future OVR SDK oculusCamController will have oculusID?
						oculusCamController.SetYRotation(-finalYawDifference.eulerAngles.y);
					}
				}
				break;
			case DriftingRotation.RazerHydra:
				// TODO
				//driftingRot = hydraRotation;
				break;
			case DriftingRotation.InputTransform:
				if(driftingTransform)
				{
					driftingRot = driftingTransform.rotation;
				}
				break;
		}
		
		if(driftingDirectionVisualizer != null)
			driftingDirectionVisualizer.transform.rotation = driftingRot;
		
		driftingEuler = driftingRot.eulerAngles;
		
		switch(compass) 
		{
			case CompassSource.Kinect:
		        if (!skeletonManager || !skeletonManager.skeletons[kinectPlayerID].isTracking)
		        {
		            break;
		        }
				else 
				{
					compassData = skeletonManager.GetJointData(compassJoint, kinectPlayerID);
				
					// First check for high confidence value
		            if (compassData != null && compassData.rotationConfidence >= 1.0f) 
					{
						updateDifferenceKalman( compassData.rotation.eulerAngles, 
												driftingEuler, deltaT 			 );
		            }
				}
				break;
			case CompassSource.PSMove:
				
		        if (inputManager)
		        {
					compassMove = inputManager.GetMoveWand(PSMoveID);
					if(compassMove)
					{
						updateDifferenceKalman( compassMove.localRotation.eulerAngles, 
												driftingEuler, deltaT 				 );
					}
				}
				break;
			case CompassSource.InputTransform:
				if(compassTransform != null)
					updateDifferenceKalman( compassTransform.rotation.eulerAngles, 
											driftingEuler, deltaT 				 );
				break;
		}
		
		float normalizedT = Mathf.Clamp01(deltaT * driftCorrectionRate);
		if(normalizedT != 0)
			finalYawDifference = Quaternion.Lerp(finalYawDifference, filteredYawDifference, 
											  normalizedT );
		
		if(correctedDirectionVisualizer != null)
			correctedDirectionVisualizer.transform.rotation = Quaternion.Euler(
											new Vector3(driftingEuler.x, 
														(360 + driftingEuler.y 
															 - finalYawDifference.eulerAngles.y)%360, 
														driftingEuler.z));
		//driftingRotation*Quaternion.Inverse(finalDifference);
		if(correctedDirectionVisualizer != null && driftVisualizerPosition != null)
			correctedDirectionVisualizer.transform.position = driftVisualizerPosition.position;
	}
	
	private void updateDifferenceKalman(Vector3 compassEuler, Vector3 driftingEuler, float deltaT)
	{
		driftingYaw = Quaternion.Euler(new Vector3(0, driftingEuler.y, 0));
		compassYaw  = Quaternion.Euler(new Vector3(0, compassEuler.y, 0));
		
		// If Kinect is used for drift correction, it can be set to apply correction only when
		// skeleton is facing the sensor
		if(compass != CompassSource.Kinect || (	  !correctOnlyWhenFacingForward 
											   || (compassYaw*Vector3.forward).z >= 0))
		{
			if(compassDirectionVisualizer != null)
			{
	            compassDirectionVisualizer.transform.rotation = compassYaw;
				if(driftVisualizerPosition != null)
					compassDirectionVisualizer.transform.position = driftVisualizerPosition.position;
			}
			if(driftingDirectionVisualizer != null && driftVisualizerPosition != null)
				driftingDirectionVisualizer.transform.position   = driftVisualizerPosition.position;
		
			// Yaw gets unstable when pitch is near poles so disregard those cases
			if(	  (driftingEuler.x < 60 || driftingEuler.x > 300)
			   && ( compassEuler.x < 60 ||  compassEuler.x > 300)) 
			{
				rotationDifference = driftingYaw * Quaternion.Inverse(compassYaw);
				yawDifferenceDirection = rotationDifference*Vector3.forward;
			
				// 2D vector rotated by yaw difference has continuous components
				measuredDrift[0] = yawDifferenceDirection.x;
				measuredDrift[1] = yawDifferenceDirection.z;
				
				filterDrift.setR(deltaT * driftNoiseCovariance);
			    filterDrift.predict();
			    filterDrift.update(measuredDrift);
				filteredDrift = filterDrift.getState();
				filteredYawDifference = 
								Quaternion.LookRotation(new Vector3((float) filteredDrift[0], 0, 
																	(float) filteredDrift[1])   );
			}
		}
	}
	
}
