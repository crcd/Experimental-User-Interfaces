/*****************************************************************************

Content    :   A basic wand used with the mouse to simulate other types of wands
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;


[AddComponentMenu("RUIS/Input/RUISMouseWand")]
public class RUISMouseWand : RUISWand {
    bool mouseButtonPressed = false;
    bool mouseButtonReleased = false;
    bool mouseButtonDown = false;
	
	public bool disableIfOtherDevices = false;

    RUISDisplayManager displayManager;

    public void Start()
    {
        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
		
		if(disableIfOtherDevices)
		{
			RUISInputManager inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
			if(inputManager)
			{
				bool otherDevices = false;
				string deviceNames = "";
				if(inputManager.enableKinect)
				{
					deviceNames += "Kinect";
					otherDevices = true;
				}
				if(inputManager.enableRazerHydra)
				{
					if(deviceNames.Length > 0)
						deviceNames += ", ";
					deviceNames += "Razer Hydra";
					otherDevices = true;
				}
						
				if(inputManager.enablePSMove)
				{
					if(deviceNames.Length > 0)
						deviceNames += ", ";
					deviceNames += "PS Move";
					otherDevices = true;
				}
				if(otherDevices)
				{
					Debug.Log(	"Disabling MouseWand GameObject '" + gameObject.name + "' because the "
							  + "following input devices were found: " + deviceNames					);
					gameObject.SetActive(false);	
				}
			}
		}
		
        if (!displayManager)
        {
            Debug.LogError("RUISMouseWand requires a RUISDisplayManager in the scene!");
        }
    }

    public void Update()
    {
        mouseButtonPressed = Input.GetMouseButtonDown(0);
        mouseButtonReleased = Input.GetMouseButtonUp(0);
        mouseButtonDown = Input.GetMouseButton(0);
    }
	
	void FixedUpdate()
	{
        Ray wandRay = displayManager.ScreenPointToRay(Input.mousePosition);
        if (wandRay.direction != Vector3.zero)
        {
			// TUUKKA:
			if (rigidbody)
        	{
            	rigidbody.MovePosition(wandRay.origin);
            	rigidbody.MoveRotation(Quaternion.LookRotation(wandRay.direction));
			}
			else
			{
				// TUUKKA: This was the original code 
	            transform.position = wandRay.origin;
	            transform.rotation = Quaternion.LookRotation(wandRay.direction);
			}
        }
	}

    public override bool SelectionButtonWasPressed()
    {
        return mouseButtonPressed;
    }

    public override bool SelectionButtonWasReleased()
    {
        return mouseButtonReleased;
    }

    public override bool SelectionButtonIsDown()
    {
        return mouseButtonDown;
    }

    public override bool IsSelectionButtonStandard()
    {
        return true;
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }
}
