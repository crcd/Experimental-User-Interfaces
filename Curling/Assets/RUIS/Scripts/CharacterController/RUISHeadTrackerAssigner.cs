/*****************************************************************************

Content    :	Leaves one head tracker enabled (that best matches RUISInputManager 
				settings) from a input list of GameObjects with RUISTracker script.
Authors    :	Tuukka Takala
Copyright  :	Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :	RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RUISHeadTrackerAssigner : MonoBehaviour {
	
	RUISInputManager inputManager;
	public List<RUISTracker> headTrackers = new List<RUISTracker>(5);
	public RUISDisplay display;
	public bool allowMultipleAssigners = false;
	public bool applyKinectDriftCorrectionPreference = false;
	public bool changePivotIfNoKinect = true;
	public Vector3 onlyRazerOffset = Vector3.zero;
//	public Vector3 onlyMouseOffset = Vector3.zero;
	public Transform razerWandParent;
	
    void Awake()
    {
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
	
    }
	
	// Use this for initialization
	void Start () 
	{
		bool kinect = false;
		bool psmove = false;
		bool razer = false;
		
		int trackerCount = 0;
		
		if(inputManager)
		{
			kinect = inputManager.enableKinect;
			psmove = inputManager.enablePSMove;
			razer  = inputManager.enableRazerHydra;
			
			RUISTracker closestMatch = null;
			int currentMatchScore = 0;

			RUISHeadTrackerAssigner[] assigners = FindObjectsOfType(typeof(RUISHeadTrackerAssigner)) as RUISHeadTrackerAssigner[];
			if(!allowMultipleAssigners && assigners.Length > 1)
			{
				Debug.LogError(  "Multiple active RUISHeadTrackerAssigner scripts found while 'Allow Multiple Assigners' is false: "
				               + "Disabling all headtrackers and their child objects that are listed in the RUISHeadTrackerAssigner "
				               + "component of '" + gameObject.name + "' object.");

				for(int i = 0; i < headTrackers.Capacity; ++i)
				{
					if(headTrackers[i] && headTrackers[i].gameObject.activeInHierarchy)
						headTrackers[i].gameObject.SetActive(false);
				}
				return;
			}

			foreach(RUISTracker trackerScript in headTrackers)
			{
				if(trackerScript && trackerScript.gameObject.activeInHierarchy)
				{
					++trackerCount;
					int foundTrackerScore = 0;
					
					// Give score to found head trackers
					if(psmove && trackerScript.headPositionInput == RUISTracker.HeadPositionSource.PSMove)
					{
						foundTrackerScore = 5;
					}
					else if(	razer && trackerScript.isRazerBaseMobile
							&&	trackerScript.headPositionInput == RUISTracker.HeadPositionSource.RazerHydra
							&&	trackerScript.mobileRazerBase == RUISTracker.RazerHydraBase.InputTransform	)
					{
						foundTrackerScore = 4;
					}
					else if(	kinect && razer && trackerScript.isRazerBaseMobile
							&&	trackerScript.headPositionInput == RUISTracker.HeadPositionSource.RazerHydra
							&&	trackerScript.mobileRazerBase == RUISTracker.RazerHydraBase.Kinect			)
					{
						foundTrackerScore = 3;
					}
					else if(kinect && trackerScript.headPositionInput == RUISTracker.HeadPositionSource.Kinect)
					{
						foundTrackerScore = 2;
					}
					else if(	razer && trackerScript.headPositionInput == RUISTracker.HeadPositionSource.RazerHydra
							&&	!trackerScript.isRazerBaseMobile															)
					{
						foundTrackerScore = 1;
					}
						
					// Assign new best head tracker candidate if it is better than the previously found
					if(currentMatchScore < foundTrackerScore)
					{
						closestMatch = trackerScript;
						currentMatchScore = foundTrackerScore;
					}
						
				}
			}
			
			if(trackerCount == 0 && Application.isEditor)
				Debug.LogError("No active GameObjects with RUISTracker script found from headTrackers list!");
			
			string positionTracker = "<None>";
			string logString = "";
			string names = "";
			RUISCamera ruisCamera = null;
			if(closestMatch == null)
			{
				// Disable all but the first active head tracker from the headTrackers list
				logString =   "Could not find a suitable head tracker with regard to "
							+ "enabled devices in RUISInputManager!";
				
				bool disabling = false;
				int leftEnabledIndex = -1;
				for(int i = 0; i < headTrackers.Capacity; ++i)
				{
					if(headTrackers[i] && headTrackers[i].gameObject.activeInHierarchy)
					{
						if(disabling)
						{
							if(names.Length > 0)
								names = names + ", ";
							names = names + headTrackers[i].gameObject.name;
							headTrackers[i].gameObject.SetActive(false);
						}
						else
						{
							leftEnabledIndex = i;
							closestMatch = headTrackers[leftEnabledIndex];
							positionTracker = headTrackers[leftEnabledIndex].gameObject.name;
							disabling = true;
						}
					}
				}
				if(leftEnabledIndex >= 0)
				{
					logString =   logString + " Choosing the first head tracker in the list. Using "
								+ positionTracker + " for tracking head position";
					if(names.Length > 0)
						logString = logString + ", and disabling the following: " + names;
					logString =   logString + ". This choice was made using a pre-selected list of "
								+ "head trackers.";
				}
				Debug.LogError(logString);
				
				ruisCamera = headTrackers[leftEnabledIndex].gameObject.GetComponentInChildren<RUISCamera>();
			}
			else
			{
				// Disable all but the closest match head tracker from the headTrackers list
				for(int i = 0; i < headTrackers.Capacity; ++i)
				{
					if(headTrackers[i] && headTrackers[i].gameObject.activeInHierarchy)
					{
						if(headTrackers[i] != closestMatch)
						{
							if(names.Length > 0)
								names = names + ", ";
							names = names + headTrackers[i].gameObject.name;
							headTrackers[i].gameObject.SetActive(false);
						}
						else
						{
							positionTracker = headTrackers[i].gameObject.name;
						}
					}
				}
				logString =   "Found the best head tracker with regard to enabled devices in "
							+ "RUISInputManager! Using " + positionTracker + " for tracking head position";
				if(names.Length > 0)
					logString = logString + ", and disabling the following: " + names;
				Debug.Log(logString + ". This choice was made using a pre-selected list of head trackers.");
				
				ruisCamera = closestMatch.gameObject.GetComponentInChildren<RUISCamera>();
				
				if(		changePivotIfNoKinect && psmove && !kinect 
					&&  closestMatch.headPositionInput == RUISTracker.HeadPositionSource.PSMove )
				{
					RUISCharacterController characterController = gameObject.GetComponentInChildren<RUISCharacterController>();
					if(		characterController != null 
						&&  characterController.characterPivotType != RUISCharacterController.CharacterPivotType.MoveController )
					{
						characterController.characterPivotType = RUISCharacterController.CharacterPivotType.MoveController;
						characterController.moveControllerId = closestMatch.positionPSMoveID;
						Debug.Log(	  "PS Move enabled and Kinect disabled. Setting " + characterController.name 
									+ "'s Character Pivot as PS Move controller #" + closestMatch.positionPSMoveID
									+ ". PS Move position offset for this pivot is " + characterController.psmoveOffset);
					}
				}
			}
			
			
			if(ruisCamera)
			{
				if(display == null)
				{
					Debug.LogError( "No RUISDisplay attached to the RUISHeadTrackerAssigner script!");
					RUISDisplay[] displays = FindObjectsOfType(typeof(RUISDisplay)) as RUISDisplay[];
					for(int i = 0; i < displays.Length; ++i)
					{
						if(displays[i].linkedCamera == null)
						{
							Debug.LogWarning(  "Assigned RUISCamera component from the child of " + positionTracker
							                 + " to render on " + displays[i].gameObject.name + " because that "
							                 + "RUISDisplay component's RUISCamera field was empty.");
							displays[i].linkedCamera = ruisCamera;
							break;
						}
					}
				}
				else
				{
					if(display.linkedCamera == null)
					{
						Debug.Log(	  "Assigned RUISCamera component from the child of " + positionTracker
									+ " to render on " + display.gameObject.name							);
						display.linkedCamera = ruisCamera;
					}
					else
						Debug.LogError(  "RUISDisplay " + display.gameObject.name + " is already connected with a "
						               + "RUISCamera object! Leave the RUISCamera field empty in your RUISDisplay "
						               + "component if you want RUISHeadTrackerAssigner script to automatically "
						               + "assign a RUISCamera to your RUISDisplay.");
				}
			}
			else
				Debug.LogError(  positionTracker + " did not have a child with RUISCamera component, "
							   + "and therefore it is not used to draw on any of the displays in "
							   + "DisplayManager.");
			
			// If we are using Razer with a static base for head tracking, then apply onlyRazerOffset
			// on the parent objects of the Razer head tracker and the hand-held Razer
			if(		closestMatch != null && razer 
				&&	closestMatch.headPositionInput == RUISTracker.HeadPositionSource.RazerHydra
				&&	!closestMatch.isRazerBaseMobile													)
			{
				// The parent object of the Razer head tracker must not have RUISCharacterConroller,
				// because that script will modify the object's position
				if(		closestMatch.transform.parent != null 
					&&	closestMatch.transform.parent.GetComponent<RUISCharacterController>() == null
					&& (onlyRazerOffset.x != 0 || onlyRazerOffset.y != 0 || onlyRazerOffset.z != 0)  )
				{
					string razerWandOffsetInfo = "";
					closestMatch.transform.parent.localPosition += onlyRazerOffset;
					if(razerWandParent != null)
					{
						razerWandParent.localPosition += onlyRazerOffset;
						razerWandOffsetInfo =  " and " + razerWandParent.gameObject.name + " (parent of hand-held Razer "
											 + "Hydra)";
					}
					Debug.Log(  "Applying offset of " + onlyRazerOffset + " to " 
							   + closestMatch.transform.parent.gameObject.name + " (parent of Razer Hydra head tracker)"
							   + razerWandOffsetInfo + "." );
				}
			}
			
			// If no Razer, Kinect, or PS Move is available, then apply onlyMouseOffset
			// on the parent object of the head tracker that is left enabled
//			if(		closestMatch != null && !razer && !kinect && !psmove)
//			{
//				// The parent object of the Razer head tracker must not have RUISCharacterConroller,
//				// because that script will modify the object's position
//				if(		closestMatch.transform.parent != null 
//					&&	closestMatch.transform.parent.GetComponent<RUISCharacterController>() == null 
//					&& (onlyMouseOffset.x != 0 || onlyMouseOffset.y != 0 || onlyMouseOffset.z != 0)  )
//				{
//					closestMatch.transform.parent.localPosition += onlyMouseOffset;
//					Debug.Log(  "Applying offset of " + onlyMouseOffset + " to " 
//							   + closestMatch.transform.parent.gameObject.name + " (parent of assigned head tracker).");
//				}
//			}
				
			// *** TODO: Below is slightly hacky
			// Read inputConfig.xml to see if Kinect yaw drift correction for Oculus Rift should be enabled
			if(	   closestMatch != null
				&& closestMatch.useOculusRiftRotation && applyKinectDriftCorrectionPreference)
			{
				if(inputManager.kinectDriftCorrectionPreferred)
				{
					// Preference is to use Kinect for drift correction (if PS Move is not used for head tracking)
					switch(closestMatch.headPositionInput)
					{
						case RUISTracker.HeadPositionSource.Kinect:
							if(!psmove && kinect)
							{
								closestMatch.externalDriftCorrection = true;
								closestMatch.compass = RUISTracker.CompassSource.Kinect;
							}
							break;
						
						case RUISTracker.HeadPositionSource.RazerHydra:
							if(!psmove && kinect && razer)
							{
								if(closestMatch.isRazerBaseMobile)
								{
									closestMatch.externalDriftCorrection = true;
									closestMatch.compass = RUISTracker.CompassSource.Kinect;
								}
							}
							break;
					}
				}
				else
				{
					// Preference is NOT to use Kinect for drift correction
					if(		closestMatch.headPositionInput == RUISTracker.HeadPositionSource.Kinect
						&&  !psmove && kinect															)
						closestMatch.externalDriftCorrection = false;
				}
			}
		}
		
		
		// When we have inferred the right RUISTracker 
		//oculusCamController = gameObject.GetComponentInChildren(typeof(OVRCameraController)) as OVRCameraController; // Is needed? ***
	}
}
