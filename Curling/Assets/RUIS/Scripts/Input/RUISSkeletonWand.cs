/*****************************************************************************

Content    :   A basic wand to use with a Kinect-tracked skeleton
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSkeletonWand")]
public class RUISSkeletonWand : RUISWand
{
    public RUISSkeletonManager.Joint wandStart = RUISSkeletonManager.Joint.RightElbow;
    public RUISSkeletonManager.Joint wandEnd = RUISSkeletonManager.Joint.RightHand;

    public RUISGestureRecognizer gestureRecognizer;

    private RUISSkeletonManager skeletonManager;

    RUISDisplayManager displayManager;

    public Color wandColor = Color.white;

    public int playerId = 0;

    private const int amountOfSelectionVisualizerImages = 8;
    Texture2D[] selectionVisualizers;

    public int visualizerWidth = 32;
    public int visualizerHeight = 32;

    public float visualizerThreshold = 0.25f;

    private RUISWandSelector wandSelector;

    private bool isTracking = false;

    public GameObject wandPositionVisualizer;

    private RUISSelectable highlightStartObject;
	
	// Tuukka:
	private Quaternion tempRotation;

    public void Awake()
    {
        if (!skeletonManager)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }

        selectionVisualizers = new Texture2D[8];
        for (int i = 0; i < amountOfSelectionVisualizerImages; i++)
        {
            selectionVisualizers[i] = Resources.Load("RUIS/Graphics/Selection/visualizer" + (i + 1)) as Texture2D;
        }

        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

        if (!gestureRecognizer)
        {
            Debug.LogWarning("Please set a gesture recognizer for wand: " + name + " if you want to use gestures.");
        }

        wandSelector = GetComponent<RUISWandSelector>();

        PlayerLost();
    }

    public void Update()
    {
        if (!isTracking && skeletonManager.skeletons[playerId].isTracking)
        {
            PlayerFound();
        }
        else if (isTracking && !skeletonManager.skeletons[playerId].isTracking)
        {
            PlayerLost();
        }
        else if (!skeletonManager.skeletons[playerId].isTracking)
        {
            return;
        }

        if (!highlightStartObject && wandSelector.HighlightedObject)
        {
            highlightStartObject = wandSelector.HighlightedObject;
            gestureRecognizer.EnableGesture();
        }
        else if (!wandSelector.HighlightedObject)
        {
            highlightStartObject = null;

            if (!wandSelector.Selection)
            {
                gestureRecognizer.DisableGesture();
            }
        }

        visualizerThreshold = Mathf.Clamp01(visualizerThreshold);

        RUISSkeletonManager.JointData startData = skeletonManager.GetJointData(wandStart, playerId);
        RUISSkeletonManager.JointData endData = skeletonManager.GetJointData(wandEnd, playerId);

        if (endData.positionConfidence >= 0.5f)
        {
			
			// TUUKKA: Original code
//            transform.localPosition = endData.position;
//
//            if (startData != null && startData.positionConfidence >= 0.5f)
//            {
//                transform.localRotation = Quaternion.LookRotation(endData.position - startData.position);
//            }
//            else if (endData.rotationConfidence >= 0.5f)
//            {
//                transform.localRotation = endData.rotation;
//            }
			
			// First calculate local rotation
            if (startData != null && startData.positionConfidence >= 0.5f)
            {
                tempRotation = Quaternion.LookRotation(endData.position - startData.position);
            }
            else if (endData.rotationConfidence >= 0.5f)
            {
                tempRotation = endData.rotation;
            }
			
			if (rigidbody)
	        {
				// TUUKKA:
				if (transform.parent)
				{
					// If the wand has a parent, we need to apply its transformation first
	            	rigidbody.MovePosition(transform.parent.TransformPoint(endData.position));
	            	rigidbody.MoveRotation(transform.parent.rotation * tempRotation);
				}
				else
				{
	            	rigidbody.MovePosition(endData.position);
	            	rigidbody.MoveRotation(tempRotation);
				}
	        }
			else
	        {
				// If there is no rigidBody, then just change localPosition & localRotation
				transform.localPosition = endData.position;
	            transform.localRotation = tempRotation;
	        }
			
        }
    }

    public void OnGUI()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return;

        float gestureProgress = gestureRecognizer.GetGestureProgress();

        if (gestureProgress >= visualizerThreshold)
        {
            float visualizerPhase = (gestureProgress - visualizerThreshold) / (1 - visualizerThreshold);
            int selectionVisualizerIndex = (int)(amountOfSelectionVisualizerImages * visualizerPhase);
            selectionVisualizerIndex = Mathf.Clamp(selectionVisualizerIndex, 0, amountOfSelectionVisualizerImages - 1);

            List<RUISDisplayManager.ScreenPoint> screenPoints = displayManager.WorldPointToScreenPoints(transform.position);

            foreach (RUISDisplayManager.ScreenPoint screenPoint in screenPoints)
            {
                RUISGUI.DrawTextureViewportSafe(new Rect(screenPoint.coordinates.x - visualizerWidth / 2, screenPoint.coordinates.y - visualizerHeight / 2, visualizerWidth, visualizerHeight),
                    screenPoint.camera, selectionVisualizers[selectionVisualizerIndex]);
                //GUI.DrawTexture(new Rect(screenPoint.x - visualizerWidth / 2, screenPoint.y - visualizerHeight / 2, visualizerWidth, visualizerHeight), selectionVisualizers[selectionVisualizerIndex]);
            }
        }

    }

    public override bool SelectionButtonWasPressed()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        if (gestureRecognizer.GestureTriggered() && wandSelector.HighlightedObject)
        {
            gestureRecognizer.ResetProgress();
            return true;
        }

        return false;
    }

    public override bool SelectionButtonWasReleased()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        if (gestureRecognizer.GestureTriggered())
        {
            gestureRecognizer.ResetProgress();
            return true;
        }

        return false;
    }

    public override bool SelectionButtonIsDown()
    {
        if (!skeletonManager.skeletons[playerId].isTracking || !gestureRecognizer) return false;
        return gestureRecognizer.GestureTriggered();
    }

    public override bool IsSelectionButtonStandard()
    {
        return false;
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }

    public override Color color { get { return wandColor; } }

    private void PlayerFound()
    {
        isTracking = true;
        gestureRecognizer.EnableGesture();
        GetComponent<LineRenderer>().enabled = true;
        if (wandPositionVisualizer)
        {
            wandPositionVisualizer.SetActive(true);
        }
    }

    private void PlayerLost()
    {
        isTracking = false;
        gestureRecognizer.DisableGesture();
        GetComponent<LineRenderer>().enabled = false;
        if (wandPositionVisualizer)
        {
            wandPositionVisualizer.SetActive(false);
        }
    }
}
