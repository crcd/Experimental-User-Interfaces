/*****************************************************************************

Content		:	Heads-up-display for presenting Oculus Rift's yaw drift calibration state
				Heavily modified version of OVRMainMenu.cs
Authors		:	Tuukka Takala, Peter Giokaris
Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

using UnityEngine;
using System.Collections;


public class RUISOculusHUD : MonoBehaviour 
{
	
	public KeyCode autoCalibrateKey = KeyCode.F5;
	public KeyCode manualCalibrateKey = KeyCode.F6;
	public KeyCode showCompassKey = KeyCode.F8;
	
	// Mag yaw-drift correction
	private RUISMagCalibration magCal = new RUISMagCalibration();
	
	// Spacing for variables that users can change
	private int    	VRVarsSX		= 553;
	private int		VRVarsSY		= 350;
	private int    	VRVarsWidthX 	= 175;
	private int    	VRVarsWidthY 	= 23;
	
	protected bool showMagStatus = false;
	private bool delayedShow = false;
	private float delayedShowTime = 2;
	private float delayedShowDuration = 2;
	private bool magCalWasDisabled = false;
	
	// Replace the GUI with our own texture and 3D plane that
	// is attached to the rendder camera for true 3D placement
	private OVRGUI  		GuiHelper 		 = new OVRGUI();
	private GameObject      GUIRenderObject  = null;
	private RenderTexture	GUIRenderTexture = null;
	
	// Set in Unity editor
	public Font textFont	= null;
	
	// Handle to OVRCameraController
	private OVRCameraController CameraController = null;
	
	void Awake()
	{
		
		magCal.autoCalibrationKey 	= autoCalibrateKey;
		magCal.manualCalibrationKey = manualCalibrateKey;
		magCal.showCompassKey 		= showCompassKey;
	}
	
	// Use this for initialization
	void Start () 
	{
		// Set the GUI target 
		GUIRenderObject = GameObject.Instantiate(Resources.Load("OVRGUIObjectMain")) as GameObject;
		
		// Find camera controller
		OVRCameraController[] CameraControllers;
		CameraControllers = FindObjectsOfType(typeof(OVRCameraController)) as OVRCameraController[];
		
		if(CameraControllers.Length == 0)
			Debug.LogWarning("OVRMainMenu: No OVRCameraController attached.");
		else if (CameraControllers.Length > 1)
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraController attached.");
		else
			CameraController = CameraControllers[0];
		
		if(GUIRenderObject != null)
		{
			if(GUIRenderTexture == null) // TODO: *** Screen.width from RUISDisplayManager
			{
				int w = Screen.width;
				int h = Screen.height;

				if(CameraController.PortraitMode == true)
				{
					int t = h;
					h = w;
					w = t;
				}
						
				GUIRenderTexture = new RenderTexture(w, h, 24);	
				GuiHelper.SetPixelResolution(w, h);
				GuiHelper.SetDisplayResolution(OVRDevice.HResolution, OVRDevice.VResolution);
			}
		}
		
		// Attach GUI texture to GUI object and GUI object to Camera
		if(GUIRenderTexture != null && GUIRenderObject != null)
		{
			GUIRenderObject.renderer.material.mainTexture = GUIRenderTexture;
			
			if(CameraController != null)
			{
				// Grab transform of GUI object
				Transform t = GUIRenderObject.transform;
				// Attach the GUI object to the camera
				CameraController.AttachGameObjectToCamera(ref GUIRenderObject);
				// Reset the transform values (we will be maintaining state of the GUI object
				// in local state)
				OVRUtils.SetLocalTransform(ref GUIRenderObject, ref t);
				// Deactivate object until we have completed the fade-in
				// Also, we may want to deactive the render object if there is nothing being rendered
				// into the UI
				// we will move the position of everything over to the left, so get
				// IPD / 2 and position camera towards negative X
				Vector3 lp = GUIRenderObject.transform.localPosition;
				float ipd = 0.0f;
				CameraController.GetIPD(ref ipd);
				lp.x -= ipd * 0.5f;
				GUIRenderObject.transform.localPosition = lp;
				
				GUIRenderObject.SetActive(false);
			}
		}
		
		// Mag Yaw-Drift correction
		magCal.SetOVRCameraController(ref CameraController);
	}
	
	/// <summary>
	/// Starts Oculus Rift's automatic calibration process for yaw drift correction
	/// </summary>
	public void StartAutoCalibration()
	{
		magCal.StartAutoCalibration();
	}
	
	/// <summary>
	/// Starts Oculus Rift's manual calibration process for yaw drift correction
	/// </summary>
	public void StartManualCalibration()
	{
		magCal.StartManualCalibration();
	}
	
	void OnGUI()
 	{	
		
		// Important to keep from skipping render events
		if (Event.current.type != EventType.Repaint)
			return;
		
		// Fade in screen
//		if(AlphaFadeValue > 0.0f)
//		{
//  			AlphaFadeValue -= Mathf.Clamp01(Time.deltaTime / FadeInTime);
//			if(AlphaFadeValue < 0.0f)
//			{
//				AlphaFadeValue = 0.0f;	
//			}
//			else
//			{
//				GUI.color = new Color(0, 0, 0, AlphaFadeValue);
//  				GUI.DrawTexture( new Rect(0, 0, Screen.width, Screen.height ), FadeInTexture ); 
//				return;
//			}
//		}
		
		// We can turn on the render object so we can render the on-screen menu
		if(GUIRenderObject != null)
		{
			if(		delayedShow || showMagStatus
				 || ((magCal.Disabled () == false) && (magCal.Ready () == false)))
				GUIRenderObject.SetActive(true);
			else
				GUIRenderObject.SetActive(false);
		}
		
		// Set the GUI matrix to deal with portrait mode
		Vector3 scale = Vector3.one;
		if(CameraController.PortraitMode == true)
		{
			float h = OVRDevice.HResolution;
			float v = OVRDevice.VResolution;
			scale.x = v / h; 					// calculate hor scale
    		scale.y = h / v; 					// calculate vert scale
		}
		Matrix4x4 svMat = GUI.matrix; // save current matrix
    	// substitute matrix - only scale is altered from standard
    	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Cache current active render texture
		RenderTexture previousActive = RenderTexture.active;
		
		// if set, we will render to this texture
		if(GUIRenderTexture != null)
		{
			RenderTexture.active = GUIRenderTexture;
			GL.Clear (false, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
		}
		
		// Update OVRGUI functions (will be deprecated eventually when 2D renderingc
		// is removed from GUI)
		GuiHelper.SetFontReplace(textFont);
		
		if(showMagStatus || delayedShow || (magCal.Disabled () == false) && (magCal.Ready () == false))
		{
			// Print out auto mag correction state
			magCal.GUIMagYawDriftCorrection(VRVarsSX, VRVarsSY, VRVarsWidthX, VRVarsWidthY,
											ref GuiHelper);
		}
		
		// Restore active render texture
		RenderTexture.active = previousActive;
		
		// ***
		// Restore previous GUI matrix
		GUI.matrix = svMat;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// After aborting calibration or stopping yaw correction,
		// we want to show the HUD for a few seconds
		magCalWasDisabled = magCal.Disabled();
		if(delayedShow)
		{
			delayedShowTime -= Time.deltaTime;
			if(delayedShowTime < 0)
				delayedShow = false;
		}
		
		magCal.UpdateMagYawDriftCorrection();
		
		// Below clause is true when aborting calibration or stopping
		// yaw correction
		if(!magCalWasDisabled && magCal.Disabled())
		{
			delayedShowTime = delayedShowDuration;
			delayedShow = true;
		}
	}
}
