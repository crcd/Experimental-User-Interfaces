/*****************************************************************************

Content    :   Comprehensive virtual reality camera class
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Xml;

public class RUISCamera : MonoBehaviour {

    [HideInInspector]
    public bool isKeystoneCorrected;

    public Camera centerCamera; //the camera used for mono rendering
    public Camera leftCamera;
    public Camera rightCamera;
	public Camera keystoningCamera;

    [HideInInspector]
    public RUISDisplay associatedDisplay;

    private Rect normalizedScreenRect;
    private float aspectRatio;

    public float horizontalFOV = 60;
    public float verticalFOV = 40;

    public LayerMask cullingMask = 0xFFFFFF;

    public bool isStereo { get { return associatedDisplay.isStereo; } }

    private bool oldStereoValue;
    private RUISDisplay.StereoType oldStereoTypeValue;

    RUISKeystoningConfiguration keystoningConfiguration;

    public float near = 0.3f;
    public float far = 1000;

	public Vector3 KeystoningHeadTrackerPosition {
        get
        {
			if(associatedDisplay && associatedDisplay.headTracker){
				return associatedDisplay.headTracker.defaultPosition;
			}
			
            return associatedDisplay.displayCenterPosition + associatedDisplay.DisplayNormal;
        }
	}

    public void Awake()
    {
        keystoningConfiguration = GetComponent<RUISKeystoningConfiguration>();

        centerCamera = camera;
        leftCamera = transform.FindChild("CameraLeft").GetComponent<Camera>();
        rightCamera = transform.FindChild("CameraRight").GetComponent<Camera>();

        centerCamera.cullingMask = cullingMask;
        leftCamera.cullingMask = cullingMask;
        rightCamera.cullingMask = cullingMask;
    }

	public void Start () {
        if (!associatedDisplay)
        {
            Debug.LogError("Camera not associated to any display, disabling... " + name, this);
            gameObject.SetActive(false);
            return;
        }

        try
        {
            if (!associatedDisplay.enableOculusRift)
            {
                GetComponent<OVRCameraController>().enabled = false;
                GetComponent<OVRDevice>().enabled = false;
                leftCamera.GetComponent<OVRCamera>().enabled = false;
                leftCamera.GetComponent<OVRLensCorrection>().enabled = false;
                rightCamera.GetComponent<OVRCamera>().enabled = false;
                rightCamera.GetComponent<OVRLensCorrection>().enabled = false;
            }
            else
            {
                foreach (RUISKeystoningBorderDrawer drawer in GetComponentsInChildren<RUISKeystoningBorderDrawer>())
                {
                    drawer.enabled = false;
                }
            }
        }
        catch (System.NullReferenceException e)
        {
            Debug.LogWarning(e.ToString(), this);
            Debug.LogWarning("Seems like the RUISCamera prefab you were using was outdated, please update... " + name, this);
        }


        UpdateStereo();
        UpdateStereoType();

        if (!leftCamera || !rightCamera)
        {
            Debug.LogError("Cameras not set properly in RUISCamera: " + name, this);
        }

        if (!associatedDisplay.enableOculusRift)
        {
        }
		
		if(associatedDisplay)
		{
            if (associatedDisplay.enableOculusRift)
            {
                if (!associatedDisplay.isStereo)
                {
                    Debug.LogError("Oculus Rift enabled in RUISCamera, forcing stereo to display: " + associatedDisplay.name, associatedDisplay);
                    associatedDisplay.isStereo = true;
                }

                associatedDisplay.isObliqueFrustum = false;
                associatedDisplay.isKeystoneCorrected = false;
            }
            else
            {
                SetupCameraTransforms();
            }
            
            if(associatedDisplay.isObliqueFrustum)
            {
                if (associatedDisplay.headTracker)
                {
                    Vector3[] eyePositions = associatedDisplay.headTracker.GetEyePositions(associatedDisplay.eyeSeparation);
				    Vector3 camToDisplay = associatedDisplay.displayCenterPosition - eyePositions[0];
        		    float distanceFromPlane = Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal);
	                if(distanceFromPlane == 0)
						Debug.LogError(  "In " + associatedDisplay.headTracker.gameObject.name + " GameObject's "
								       + "RUISTracker script, you have set defaultPosition to " 
								       + "lie on the display plane of " 
								       + associatedDisplay.gameObject.name + ". The defaultPosition "
								       + "needs to be apart from the display!", associatedDisplay);
                }
                else
                {
                    Debug.LogError("RUISTracker is none, you need to set it from the inspector!", associatedDisplay);
                }
            }
		}
	}
	
	public void Update () {
			
        if (oldStereoValue != associatedDisplay.isStereo)
        {
            UpdateStereo();
        }

        if (oldStereoTypeValue != associatedDisplay.stereoType)
        {
            UpdateStereoType();
        }
	}

    public void LateUpdate()
    {
        if(associatedDisplay.enableOculusRift){
            return;
        }

        centerCamera.ResetProjectionMatrix();
        leftCamera.ResetProjectionMatrix();
        rightCamera.ResetProjectionMatrix();

        SetupCameraTransforms();


        Matrix4x4[] projectionMatrices = GetProjectionMatricesWithoutKeystoning();
        centerCamera.projectionMatrix = projectionMatrices[0];
        leftCamera.projectionMatrix = projectionMatrices[1];
        rightCamera.projectionMatrix = projectionMatrices[2];

        if (associatedDisplay.isObliqueFrustum)
        {
            centerCamera.worldToCameraMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale).inverse;

            leftCamera.worldToCameraMatrix = centerCamera.worldToCameraMatrix;
            rightCamera.worldToCameraMatrix = centerCamera.worldToCameraMatrix;
        }

        ApplyKeystoneCorrection();
    }

    public Matrix4x4[] GetProjectionMatricesWithoutKeystoning()
    {
        if (associatedDisplay.isObliqueFrustum && associatedDisplay.headTracker)
        {
            Vector3[] eyePositions = associatedDisplay.headTracker.GetEyePositions(associatedDisplay.eyeSeparation);
            return new Matrix4x4[] { CreateProjectionMatrix(eyePositions[0]), 
                                     CreateProjectionMatrix(eyePositions[1]),
                                     CreateProjectionMatrix(eyePositions[2]) };
        }
        else
        {
            Matrix4x4 defaultMatrix = CreateDefaultFrustum();
            return new Matrix4x4[] { defaultMatrix, defaultMatrix, defaultMatrix };
        }
    }

    //http://csc.lsu.edu/~kooima/pdfs/gen-perspective.pdf
    //Generalized Perspective Projection
    //Robert Kooima
    public Matrix4x4 CreateProjectionMatrix(Vector3 trackerCoordinates)
    {
            Vector3 va = associatedDisplay.BottomLeftPosition - trackerCoordinates;
            Vector3 vb = associatedDisplay.BottomRightPosition - trackerCoordinates;
            Vector3 vc = associatedDisplay.TopLeftPosition - trackerCoordinates;
            Vector3 vr = associatedDisplay.DisplayRight;
            Vector3 vu = associatedDisplay.DisplayUp;
            Vector3 vn = associatedDisplay.DisplayNormal;

            float eyedistance = -(Vector3.Dot(va, vn));

            float left = (Vector3.Dot(vr, va) * near) / eyedistance;
            float right = (Vector3.Dot(vr, vb) * near) / eyedistance;
            float bottom = (Vector3.Dot(vu, va) * near) / eyedistance;
            float top = (Vector3.Dot(vu, vc) * near) / eyedistance;
            Matrix4x4 projectionMatrix = CreateFrustum(left, right, bottom, top, near, far);

            Matrix4x4 rotation = Matrix4x4.identity;
            rotation.SetRow(0, vr);
            rotation.SetRow(1, vu);
            rotation.SetRow(2, vn);
		
            Matrix4x4 translation = Matrix4x4.identity;
            translation.SetColumn(3, -trackerCoordinates);
            translation[3, 3] = 1;
		 
            return projectionMatrix * rotation * translation;

            //return projectionMatrix;

        /*Vector3 camToDisplay = associatedDisplay.displayCenterPosition - trackerCoordinates;
        float dd = Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal);
        float frx = -Vector3.Dot(camToDisplay, associatedDisplay.DisplayRight) / dd;
        float fry = -Vector3.Dot(camToDisplay, associatedDisplay.DisplayUp) / dd;
        float scx = dd / (0.5f * associatedDisplay.width); // Metreissä
        float scy = dd / (0.5f * associatedDisplay.height); // Metreissä
        if (dd > 0)
            scy *= -1;

        Matrix4x4 B = Matrix4x4.identity;
        B[0, 0] = scx;
        B[1, 1] = scy;
        B[0, 2] = scx * frx;
        B[1, 2] = scy * fry;

        
        //Vector3 eyeProjWall = associatedDisplay.DisplayNormal * Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal);
        float eyeProjWallX = trackerCoordinates.x - associatedDisplay.DisplayNormal.x * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal)); // Tässä on jostain syystä miinusmerkki
        float eyeProjWallY = trackerCoordinates.y + associatedDisplay.DisplayNormal.y * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal));
        float eyeProjWallZ = trackerCoordinates.z + associatedDisplay.DisplayNormal.z * (Vector3.Dot(camToDisplay, associatedDisplay.DisplayNormal));


        Matrix4x4 C = Camera(trackerCoordinates.x, trackerCoordinates.y, trackerCoordinates.z, eyeProjWallX, eyeProjWallY, eyeProjWallZ, associatedDisplay.DisplayUp.x, associatedDisplay.DisplayUp.y, associatedDisplay.DisplayUp.z);

        return CreateDefaultFrustum() * B * C;*/
    }

    public Matrix4x4 CreateDefaultFrustum()
    {
        float right = -Mathf.Tan(horizontalFOV / 2 * Mathf.Deg2Rad) * near;
        float left = -right;
        float top = -Mathf.Tan(verticalFOV / 2 * Mathf.Deg2Rad) * near;
        float bottom = -top;

        return CreateFrustum(right, left, top, bottom, near, far);
    }
	
	public Matrix4x4 CreateKeystoningObliqueFrustum(){
		return CreateProjectionMatrix (KeystoningHeadTrackerPosition);
	}

    private static Matrix4x4 CreateFrustum(float left, float right, float bottom, float top, float near, float far)
    {
        Matrix4x4 frustum = new Matrix4x4();

        frustum[0, 0] = 2 * near / (right - left);
        frustum[0, 2] = (right + left) / (right - left);
        frustum[1, 1] = 2 * near / (top - bottom);
        frustum[1, 2] = (top + bottom) / (top - bottom);
        frustum[2, 2] = -(far + near) / (far - near);
        frustum[2, 3] = -2 * far * near / (far - near);
        frustum[3, 2] = -1;

        return frustum;
    }

    public void SetupKeystoneCorrection()
    {
        isKeystoneCorrected = true;
    }

    private void ApplyHeadTrackingDistortion()
    {
    }

    private void ApplyKeystoneCorrection()
    {
        //Debug.Log(keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix());
        //Debug.Log(centerCamera.projectionMatrix * keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix());
        centerCamera.projectionMatrix = keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix() * centerCamera.projectionMatrix;
        leftCamera.projectionMatrix = keystoningConfiguration.leftCameraKeystoningSpec.GetMatrix() * leftCamera.projectionMatrix;
        rightCamera.projectionMatrix = keystoningConfiguration.rightCameraKeystoningSpec.GetMatrix() * rightCamera.projectionMatrix;
        //Debug.Log(keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix());
        //leftCamera.projectionMatrix *= keystoningConfiguration.leftCameraKeystoningSpec.GetMatrix();
        //rightCamera.projectionMatrix *= keystoningConfiguration.rightCameraKeystoningSpec.GetMatrix();
    }

    virtual public void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight, float aspectRatio)
    {
        normalizedScreenRect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
        this.aspectRatio = aspectRatio;

        centerCamera.rect = normalizedScreenRect;
        centerCamera.aspect = aspectRatio;

        if (associatedDisplay == null)
        {
            Debug.LogError("Associated Display was null", this);
        }

        if (associatedDisplay.stereoType == RUISDisplay.StereoType.SideBySide)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth / 2, relativeHeight);
            rightCamera.rect = new Rect(relativeLeft + relativeWidth / 2, relativeBottom, relativeWidth / 2, relativeHeight);
        }
        else if (associatedDisplay.stereoType == RUISDisplay.StereoType.TopAndBottom)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom + relativeHeight / 2, relativeWidth, relativeHeight / 2);
            rightCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight / 2);
        }
        else
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
            rightCamera.rect = new Rect(leftCamera.rect);
        }

        leftCamera.aspect = aspectRatio;
        rightCamera.aspect = aspectRatio;
    }

    private void SetupCameraTransforms()
    {
        float halfEyeSeparation = associatedDisplay.eyeSeparation / 2;
        leftCamera.transform.localPosition = new Vector3(-halfEyeSeparation, 0, 0);
        rightCamera.transform.localPosition = new Vector3(halfEyeSeparation, 0, 0);

        /*if (zeroParallaxDistance > 0)
        {
            float angle = Mathf.Acos(halfEyeSeparation / Mathf.Sqrt(Mathf.Pow(halfEyeSeparation, 2) + Mathf.Pow(zeroParallaxDistance, 2)));
            Vector3 rotation = new Vector3(0, angle, 0);
            rightCamera.transform.localRotation = Quaternion.Euler(-rotation);
            leftCamera.transform.localRotation = Quaternion.Euler(rotation);
        }*/
    }

    private void UpdateStereo()
    {
        if (associatedDisplay.isStereo)
        {
            centerCamera.enabled = false;
            leftCamera.enabled = true;
            rightCamera.enabled = true;
        }
        else
        {
            centerCamera.enabled = true;
            leftCamera.enabled = false;
            rightCamera.enabled = false;
        }

        oldStereoValue = associatedDisplay.isStereo;
    }

    private void UpdateStereoType()
    {
        SetupCameraViewports(normalizedScreenRect.xMin, normalizedScreenRect.yMin, normalizedScreenRect.width, normalizedScreenRect.height, aspectRatio);
        oldStereoTypeValue = associatedDisplay.stereoType;
    }

    public void LoadKeystoningFromXML(XmlDocument xmlDoc)
    {
        keystoningConfiguration.LoadFromXML(xmlDoc);
    }

    public void SaveKeystoningToXML(XmlElement displayXmlElement)
    {
        keystoningConfiguration.SaveToXML(displayXmlElement);
    }

	/*
    public void OnDrawGizmos()
    {
        if (!associatedDisplay) return;

        Color color = Gizmos.color;
        Gizmos.color = new Color(50, 50, 50);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.TopRightPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.BottomRightPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.BottomLeftPosition);
        Gizmos.DrawLine(headTracker.transform.position, associatedDisplay.TopLeftPosition);


        Matrix4x4 originalMatrix = Gizmos.matrix;
        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetTRS(Vector3.zero, Quaternion.LookRotation(associatedDisplay.DisplayNormal, associatedDisplay.DisplayUp), Vector3.one);
        //Gizmos.matrix = rotationMatrix;

        Gizmos.DrawCube((associatedDisplay.TopRightPosition + associatedDisplay.BottomRightPosition) / 2, new Vector3(0.1f, associatedDisplay.TopRightPosition.y - associatedDisplay.BottomRightPosition.y, 0.1f));
        Gizmos.DrawCube((associatedDisplay.TopLeftPosition + associatedDisplay.BottomLeftPosition) / 2, new Vector3(0.1f, associatedDisplay.TopLeftPosition.y - associatedDisplay.BottomLeftPosition.y, 0.1f));
        Gizmos.DrawCube((associatedDisplay.TopRightPosition + associatedDisplay.TopLeftPosition) / 2, new Vector3(associatedDisplay.TopRightPosition.x - associatedDisplay.TopLeftPosition.x, 0.1f, 0.1f));
        Gizmos.DrawCube((associatedDisplay.BottomRightPosition + associatedDisplay.BottomLeftPosition) / 2, new Vector3(associatedDisplay.BottomRightPosition.x - associatedDisplay.BottomLeftPosition.x, 0.1f, 0.1f));
        
        Gizmos.color = color;
        Gizmos.matrix = originalMatrix;
    }
	 */
}
