/*****************************************************************************

Content    :   A manager for display configurations
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public class RUISDisplayManager : MonoBehaviour {
    public List<RUISDisplay> displays;
    public GameObject stereoCamera;
    public Camera monoCamera;
    public int totalResolutionX = 0;
    public int totalResolutionY = 0;
    public int totalRawResolutionX = 0;
    public int totalRawResolutionY = 0;

    public bool allowResolutionDialog;

    public class ScreenPoint
    {
        public Vector2 coordinates;
        public Camera camera;
    }

	void Start () {

        CalculateTotalResolution();

        if (Application.isEditor)
        {
            UpdateResolutionsOnTheFly();
        }

        UpdateDisplays();

        DisableUnlinkedCameras();


        LoadDisplaysFromXML();
	}

    void Update()
    {
        if (Application.isEditor && (Screen.width != totalRawResolutionX || Screen.height != totalRawResolutionY))
        {
            UpdateResolutionsOnTheFly();
            UpdateDisplays();
        }
    }

    public void UpdateDisplays()
    {
        CalculateTotalResolution();

        int currentResolutionX = 0;
        foreach (RUISDisplay display in displays)
        {
            display.SetupViewports(currentResolutionX, new Vector2(totalRawResolutionX, totalRawResolutionY));
            currentResolutionX += display.rawResolutionX;
        }

        if (displays.Count > 1 || (displays.Count == 0 && !allowResolutionDialog))
        {
            Screen.SetResolution(totalRawResolutionX, totalRawResolutionY, false);
        }
    }

    public void CalculateTotalResolution()
    {
        totalResolutionX = 0;
        totalResolutionY = 0;
        totalRawResolutionX = 0;
        totalRawResolutionY = 0;

        foreach (RUISDisplay display in displays)
        {
            totalResolutionX += display.resolutionX;
            totalResolutionY = Mathf.Max(totalResolutionY, display.resolutionY);

            totalRawResolutionX += display.rawResolutionX;
            totalRawResolutionY = Mathf.Max(totalRawResolutionY, display.rawResolutionY);
        }
    }

    public Ray ScreenPointToRay(Vector2 screenPoint)
    {
        RUISDisplay display = GetDisplayForScreenPoint(screenPoint);


        if (display)
        {
            Camera camera = display.GetCameraForScreenPoint(screenPoint);
            
            if (camera)
            {   
                return camera.ScreenPointToRay(screenPoint);
            }
        }
         
        return new Ray(Vector3.zero, Vector3.zero);
    }

    public List<ScreenPoint> WorldPointToScreenPoints(Vector3 worldPoint)
    {
        List<ScreenPoint> screenPoints = new List<ScreenPoint>();

        foreach (RUISDisplay display in displays)
        {
            display.WorldPointToScreenPoints(worldPoint, ref screenPoints);
        }

        return screenPoints;
    }

    public RUISDisplay GetDisplayForScreenPoint(Vector2 screenPoint/*, ref Vector2 relativeScreenPoint*/)
    {
        //relativeScreenPoint = Vector2.zero;

         int currentResolutionX = 0;
         foreach (RUISDisplay display in displays)
         {

             if (currentResolutionX + display.rawResolutionX >= screenPoint.x)
             {
                 //relativeScreenPoint = new Vector2(screenPoint.x - currentResolutionX, totalRawResolutionY - screenPoint.y);
                 return display;
             }

             currentResolutionX += display.rawResolutionX;
         }

         return null;
    }
    /*
    public Camera GetCameraForScreenPoint(Vector2 screenPoint)
    {
        Vector2 relativeScreenPoint = Vector2.zero;
        RUISDisplay display = GetDisplayForScreenPoint(screenPoint);
        //Debug.Log(relativeScreenPoint);
        if (display)
            return display.GetCameraForScreenPoint(relativeScreenPoint, totalRawResolutionY);
        else
            return null;
    }*/

    private void UpdateResolutionsOnTheFly()
    {
        int trueWidth = Screen.width;
        int trueHeight = Screen.height;

        float widthScaler = (float)trueWidth / totalRawResolutionX;
        float heightScaler = (float)trueHeight / totalRawResolutionY;

        foreach (RUISDisplay display in displays)
        {
            display.resolutionX = (int)(display.resolutionX * widthScaler);
            display.resolutionY = (int)(display.resolutionY * heightScaler);
        }
    }

    private void DisableUnlinkedCameras()
    {
        RUISCamera[] allCameras = FindObjectsOfType(typeof(RUISCamera)) as RUISCamera[];

        foreach (RUISCamera ruisCamera in allCameras)
        {
            if (ruisCamera.associatedDisplay == null)
            {
                Debug.LogWarning("Disabling RUISCamera '" + ruisCamera.name + "' because it isn't linked into a RUISDisplay.");
                ruisCamera.gameObject.SetActive(false);
            }
        }
    }

    public void LoadDisplaysFromXML(bool refresh = false)
    {
        foreach (RUISDisplay display in displays)
        {
            display.LoadFromXML();
        }

        if (refresh)
        {
            UpdateDisplays();
        }
    }

    public void SaveDisplaysToXML()
    {
        foreach (RUISDisplay display in displays)
        {
            display.SaveToXML();
        }
    }

    public RUISDisplay GetOculusRiftDisplay()
    {
        foreach (RUISDisplay display in displays)
        {
            if (display.linkedCamera && display.enableOculusRift)
            {
                return display;
            }
        }

        return null;
    }
}
