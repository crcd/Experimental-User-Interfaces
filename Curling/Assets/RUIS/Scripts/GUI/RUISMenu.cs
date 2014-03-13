/*****************************************************************************

Content    :   The menu shown when pressing F2 (default)
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

[AddComponentMenu("RUIS/GUI/RUISMenu")]
public class RUISMenu : MonoBehaviour {
    private enum MenuState
    {
        Main,
        Calibrating,
        EditingDisplays,
        EditingInputConfiguration
    }
	
	public int fontSize = 14;
	public float stereoOffset = -110;
	
    private MenuState menuState = MenuState.Main;

    private int currentWindow = 0;
    private Rect windowRect = new Rect(50, 50, 250, 250);
	
	public GUISkin menuSkin;
	private Color originalBackground;
	private GUIStyle gridStyle;
	private Color darkGreen = new Color(0, 0.8f, 0);
	private Color lightGrey = new Color(0.8f, 0.8f, 0.8f);

    bool isShowing = false;

    bool ruisMenuButtonDefined = true;

    int previousSceneId = -1;

    [HideInInspector] public bool enablePSMove;
    [HideInInspector] public string psMoveIP;
    [HideInInspector] public int psMovePort;

    bool isEditingKeystones = false;

    RUISInputManager inputManager;

    public RUISJumpGestureRecognizer jumpGesture;

    private bool originalEnablePSMove;
    private bool originalEnableKinect;
    private bool originalEnableJumpGesture;
    private bool originalEnableHydra;
	private bool originalKinectDriftCorrection;
    private RUISInputManager.RiftMagnetometer originalMagnetometerMode;

    public bool oculusRiftMenu = false;
    public bool hideMouseOnPlay = true;
    RUISDisplayManager displayManager;
    RUISDisplay riftDisplay;

	// Use this for initialization
	void Start () {

        try
        {
            Input.GetButtonDown("RUISMenu");
        }
        catch (UnityException)
        {
            ruisMenuButtonDefined = false;
        }
		
        inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        enablePSMove = inputManager.enablePSMove;
        psMoveIP = inputManager.PSMoveIP;
        psMovePort = inputManager.PSMovePort;

        jumpGesture = FindObjectOfType(typeof(RUISJumpGestureRecognizer)) as RUISJumpGestureRecognizer;

        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
        riftDisplay = displayManager.GetOculusRiftDisplay();
		
		// TODO: This menu should work with any stereo view, not just Rift. riftDisplay.linkedCamera is null when Rift is disabled.
		if (oculusRiftMenu && riftDisplay && riftDisplay.linkedCamera )
        {
            windowRect = new Rect(riftDisplay.linkedCamera.leftCamera.pixelRect.x 
									+ riftDisplay.resolutionX / 4 - 100, riftDisplay.resolutionY / 2 - 220, 250, 250);
        }
		
        SaveInputChanges();
	}

    void Update()
    {
        if ((!ruisMenuButtonDefined && Input.GetKeyDown(KeyCode.Escape)) || (ruisMenuButtonDefined && Input.GetButtonDown("RUISMenu")))
        {
            if (isShowing)
            {
                DiscardInputChanges();

            }

            isShowing = !isShowing;
        }
    }

    void OnGUI()
    {
        if (!isShowing) 
		{
			if(hideMouseOnPlay && !Application.isEditor)
				Screen.showCursor = false;
			return;
		}
		Screen.showCursor = true;
		
		GUI.skin = menuSkin; // *** TODO: Now setting colors for active buttons in script <-- unnecessary. Adjust everything in the skin.
		originalBackground = GUI.backgroundColor; 
		
		menuSkin.box.fontSize 		= fontSize;
		menuSkin.button.fontSize 	= fontSize;
		menuSkin.label.fontSize 	= fontSize;
		menuSkin.textArea.fontSize 	= fontSize;
		menuSkin.textField.fontSize = fontSize;
		menuSkin.toggle.fontSize 	= fontSize;
		menuSkin.window.fontSize 	= fontSize;
		
        windowRect = GUILayout.Window(currentWindow, windowRect, DrawWindow, "RUIS");

        if (oculusRiftMenu)
        {
			if (riftDisplay)
            {
                float offset = riftDisplay.resolutionX / 2 + stereoOffset;
                Rect temp = GUILayout.Window(currentWindow + 1, new Rect(windowRect.x + offset, windowRect.y, 
												windowRect.width, windowRect.height), DrawWindow, "RUIS");
                windowRect = new Rect(temp.x - offset, temp.y, temp.width, temp.height);
            }
        }
    }

    void DrawWindow(int windowId)
    {
		Vector2 elementSize;
		float elementHeight = 10;
		
		elementSize = GUI.skin.GetStyle("button").CalcSize(new GUIContent("X"));
		float additionalSpacing = 0.5f*elementSize.y;
		GUILayout.Space(additionalSpacing);
		
        switch(menuState){
            case MenuState.Main:
                inputManager.enablePSMove = GUILayout.Toggle(inputManager.enablePSMove, "Use PS Move");
                
			
				elementSize = GUI.skin.GetStyle("textField").CalcSize(new GUIContent("999099909990999"));
                GUI.enabled = inputManager.enablePSMove;
                GUILayout.BeginHorizontal();
                    GUILayout.Label("IP:");
                    string ipText = GUILayout.TextField(inputManager.PSMoveIP, GUILayout.Width(elementSize.x));
                GUILayout.EndHorizontal();
                ipText = Regex.Replace(ipText, @"[^0-9 .]", "");
                inputManager.PSMoveIP = ipText;
                GUILayout.BeginHorizontal();
                    GUILayout.Label("Port:");
                    string portText = GUILayout.TextArea(inputManager.PSMovePort.ToString(), GUILayout.Width(elementSize.x));
                GUILayout.EndHorizontal();
                portText = Regex.Replace(portText, @"[^0-9 ]", "");
                inputManager.PSMovePort = int.Parse(portText);
                GUI.enabled = true;

                inputManager.enableKinect = GUILayout.Toggle(inputManager.enableKinect, "Use Kinect");

                GUI.enabled = inputManager.enablePSMove && inputManager.enableKinect;

				GUI.backgroundColor = originalBackground;
                if (GUILayout.Button("Calibrate PS Move & Kinect (and Save)"))
                {
                    inputManager.Export(inputManager.filename);
                    SaveInputChanges();

                    DontDestroyOnLoad(this);

                    Debug.Log("Loading calibration screen.");

                    gameObject.transform.parent = null;

                    previousSceneId = Application.loadedLevel;

                    menuState = MenuState.Calibrating;

                    isShowing = false;

                    Application.LoadLevel("calibration");
                }

                GUI.enabled = true;

                
                inputManager.enableRazerHydra = GUILayout.Toggle(inputManager.enableRazerHydra, "Use Hydra");
			
				GUI.backgroundColor = originalBackground;
			
				gridStyle = new GUIStyle(GUI.skin.button);
				gridStyle.onActive.textColor = darkGreen;
				gridStyle.onNormal.textColor = darkGreen;
			
                GUILayout.Space(additionalSpacing);
				GUI.color = lightGrey;
                GUILayout.Label("Rift magnetometer drift correction:");
				GUI.color = Color.white;
                string[] magnetometerNames = System.Enum.GetNames(typeof(RUISInputManager.RiftMagnetometer));
                inputManager.riftMagnetometerMode = (RUISInputManager.RiftMagnetometer)GUILayout.SelectionGrid((int)inputManager.riftMagnetometerMode, 
																												magnetometerNames, 1, gridStyle);
			
                GUILayout.Space(additionalSpacing);

                if (inputManager.enableKinect && !inputManager.enablePSMove)
                {
					if(inputManager.kinectDriftCorrectionPreferred)
						GUI.backgroundColor = Color.green;
					elementSize = GUI.skin.GetStyle("label").CalcSize(new GUIContent("XX"));
					GUILayout.BeginHorizontal();
					GUILayout.Space(elementSize.x + 0.5f*GUI.skin.label.margin.vertical);
                    inputManager.kinectDriftCorrectionPreferred = GUILayout.Toggle(inputManager.kinectDriftCorrectionPreferred, " Use Kinect For Drift Correction");
				  	GUILayout.EndHorizontal();
					GUI.backgroundColor = originalBackground;
                }
                else
                {
					elementSize = GUI.skin.GetStyle("toggle").CalcSize(new GUIContent(" Use Kinect For Drift Correction"));
					elementHeight = GUI.skin.GetStyle("toggle").CalcHeight(new GUIContent(" Use Kinect For Drift Correction"), elementSize.x);
                    GUILayout.Space(elementHeight + 0.5f*GUI.skin.toggle.margin.vertical);
                }
			
                GUILayout.Space(2*additionalSpacing);

                if (GUILayout.Button("Display Management"))
                {
                    SwitchKeystoneEditingState();
                    //ToggleKeystoneGridState();
                    menuState = MenuState.EditingDisplays;
                }

                GUI.enabled = UnsavedChanges();
                if (GUILayout.Button("     Save Configuration & Restart Scene"))
                {
                    inputManager.Export(inputManager.filename);
                    SaveInputChanges();
                    Application.LoadLevel(Application.loadedLevel);
                }
                if (GUILayout.Button("Discard Configuration"))
                {
                    menuState = MenuState.Main;
                    DiscardInputChanges();
					isShowing = !isShowing;
                }
                GUI.enabled = true;
                /*if (GUILayout.Button("Display Configuration"))
                {
                    SwitchKeystoneEditingState();
                    menuState = MenuState.EditingDisplays;
                }
                if (GUILayout.Button("Input Configuration"))
                {
                    menuState = MenuState.EditingInputConfiguration;
                }
				if(GUILayout.Button ("Resize Screen")){
					(FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).UpdateDisplays();
				}*/
                if (GUILayout.Button("Quit Application"))
                {
                    Application.Quit();
                }
                break;
            case MenuState.Calibrating:
                if(GUILayout.Button("End Calibration")){
                    Destroy(this.gameObject);

                    Application.LoadLevel(previousSceneId);
                }
                break;
            case MenuState.EditingDisplays:
                if (GUILayout.Button("Reset Keystoning"))
                {
                    foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
                    {
                        keystoningConfiguration.ResetConfiguration();
                    }
                }
                if(GUILayout.Button("Save Configurations")){
                    (FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).SaveDisplaysToXML();
                }
                if(GUILayout.Button("Load Old Configurations")){
                    (FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).LoadDisplaysFromXML();
                }
                if (GUILayout.Button("End Display Editing"))
                {
                    SwitchKeystoneEditingState();
                    menuState = MenuState.Main;
                }
                break;
            case MenuState.EditingInputConfiguration:
                
                break;
        }
		
		originalBackground = GUI.backgroundColor;
        GUI.DragWindow();
    }

    private void SwitchKeystoneEditingState()
    {
        foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
        {
            if (isEditingKeystones)
            {
                keystoningConfiguration.EndEditing();
            }
            else
            {
                keystoningConfiguration.StartEditing();
            }
        }

        isEditingKeystones = !isEditingKeystones;
    }

    private void ToggleKeystoneGridState()
    {
        foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
        {
            keystoningConfiguration.drawKeystoningGrid = !keystoningConfiguration.drawKeystoningGrid;
        }
    }

    private void SaveInputChanges()
    {
        originalEnablePSMove = inputManager.enablePSMove;
        originalEnableKinect = inputManager.enableKinect;
        originalEnableJumpGesture = inputManager.jumpGestureEnabled;
        originalEnableHydra = inputManager.enableRazerHydra;
        originalMagnetometerMode = inputManager.riftMagnetometerMode;
		originalKinectDriftCorrection = inputManager.kinectDriftCorrectionPreferred;
    }

    private void DiscardInputChanges()
    {
        inputManager.enablePSMove = originalEnablePSMove;
        inputManager.enableKinect = originalEnableKinect;
        if (jumpGesture)
        {
            if (originalEnableJumpGesture)
            {
                jumpGesture.EnableGesture();
            }
            else
            {
                jumpGesture.DisableGesture();
            }
        }
		inputManager.jumpGestureEnabled = originalEnableJumpGesture;
        inputManager.enableRazerHydra = originalEnableHydra;
        inputManager.riftMagnetometerMode = originalMagnetometerMode;
		inputManager.kinectDriftCorrectionPreferred = originalKinectDriftCorrection;
    }

    private bool UnsavedChanges()
    {
        return originalEnablePSMove != inputManager.enablePSMove ||
        originalEnableKinect != inputManager.enableKinect ||
        originalEnableJumpGesture != inputManager.jumpGestureEnabled ||
        originalEnableHydra != inputManager.enableRazerHydra ||
        originalMagnetometerMode != inputManager.riftMagnetometerMode ||
		originalKinectDriftCorrection != inputManager.kinectDriftCorrectionPreferred;
    }
}

