/*****************************************************************************

Content    :   The main class used for coordinate system transforms between different input systems and Unity
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.IO;

public class RUISCoordinateSystem : MonoBehaviour
{
    public string coordinateXmlFile = "calibration.xml";
    public TextAsset coordinateSchema;
    public bool loadFromXML = true;

    public const float kinectToUnityScale = 0.001f;
    public const float moveToUnityScale = 0.1f;

    public Vector3 kinectFloorNormal { get; private set; }
    private Quaternion kinectFloorRotator = Quaternion.identity;

    private Matrix4x4 moveToRUISTransform = Matrix4x4.identity;
    private Quaternion moveToKinectRotation = Quaternion.identity;
    //private Matrix4x4 kinectToRUISTransform = Matrix4x4.identity;
    //private Quaternion kinectToRUISRotation = Quaternion.identity;

    private float kinectDistanceFromFloor = 0;
    //private Quaternion kinectYaw = Quaternion.identity;

    public bool applyMoveToKinect = true;
    public bool setKinectOriginToFloor = false;

    public Vector3 positionOffset = Vector3.zero;
    public float yawOffset = 0;

    void Start()
    {
        SetMoveToKinectTransforms(Matrix4x4.identity, Matrix4x4.identity);

        if (loadFromXML)
        {
            if (!LoadXML(coordinateXmlFile))
            {
                Debug.LogError("Could not load calibration file. Creating default file.");
                SaveXML(coordinateXmlFile);
            }
        }
    }

    //Load the transformation data from an xml file created by the calibration part
    public bool LoadXML(string filename)
    {
        XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, coordinateSchema);
        if (xmlDoc == null)
        {
            return false;
        }


        Matrix4x4 moveToKinectTransform = new Matrix4x4();
        XmlNode translationElement = xmlDoc.GetElementsByTagName("translate").Item(0);
        float x = float.Parse(translationElement.Attributes["x"].Value);
        float y = float.Parse(translationElement.Attributes["y"].Value);
        float z = float.Parse(translationElement.Attributes["z"].Value);
        moveToKinectTransform.SetColumn(3, new Vector4(x, y, z, 1.0f));

        XmlNode rotationElement = xmlDoc.GetElementsByTagName("rotate").Item(0);
        moveToKinectTransform.m00 = float.Parse(rotationElement.Attributes["r00"].Value);
        moveToKinectTransform.m01 = float.Parse(rotationElement.Attributes["r01"].Value);
        moveToKinectTransform.m02 = float.Parse(rotationElement.Attributes["r02"].Value);
        moveToKinectTransform.m10 = float.Parse(rotationElement.Attributes["r10"].Value);
        moveToKinectTransform.m11 = float.Parse(rotationElement.Attributes["r11"].Value);
        moveToKinectTransform.m12 = float.Parse(rotationElement.Attributes["r12"].Value);
        moveToKinectTransform.m20 = float.Parse(rotationElement.Attributes["r20"].Value);
        moveToKinectTransform.m21 = float.Parse(rotationElement.Attributes["r21"].Value);
        moveToKinectTransform.m22 = float.Parse(rotationElement.Attributes["r22"].Value);

        moveToRUISTransform = moveToKinectTransform;

        List<Vector3> rotationVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(moveToKinectTransform));
        Matrix4x4 rotationMatrix = new Matrix4x4();
        rotationMatrix.SetColumn(0, rotationVectors[0]);
        rotationMatrix.SetColumn(1, rotationVectors[1]);
        rotationMatrix.SetColumn(2, rotationVectors[2]);

        moveToKinectRotation = MathUtil.QuaternionFromMatrix(rotationMatrix);

        XmlNode kinectFloorNormalElement = xmlDoc.GetElementsByTagName("kinectFloorNormal").Item(0);

        if(kinectFloorNormalElement != null)
        {
            SetKinectFloorNormal(new Vector3(float.Parse(kinectFloorNormalElement.Attributes["x"].Value),
                                            float.Parse(kinectFloorNormalElement.Attributes["y"].Value),
                                            float.Parse(kinectFloorNormalElement.Attributes["z"].Value)));
        }

        XmlNode kinectDistanceFromFloorElement = xmlDoc.GetElementsByTagName("kinectDistanceFromFloor").Item(0);

        if (kinectDistanceFromFloorElement != null)
        {
            kinectDistanceFromFloor = float.Parse(kinectDistanceFromFloorElement.Attributes["value"].Value);
        }

        return true;
    }

    public void SaveXML(string filename)
    {
        XmlDocument xmlDoc = new XmlDocument();

        xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

        XmlElement m2kElement = xmlDoc.CreateElement("ns2", "m2k", "http://ruisystem.net/m2k");
        xmlDoc.AppendChild(m2kElement);

        XmlElement translateElement = xmlDoc.CreateElement("translate");
        translateElement.SetAttribute("x", moveToRUISTransform[0, 3].ToString());
        translateElement.SetAttribute("y", moveToRUISTransform[1, 3].ToString());
        translateElement.SetAttribute("z", moveToRUISTransform[2, 3].ToString());

        m2kElement.AppendChild(translateElement);

        XmlElement rotateElement = xmlDoc.CreateElement("rotate");
        rotateElement.SetAttribute("r00", moveToRUISTransform[0, 0].ToString());
        rotateElement.SetAttribute("r01", moveToRUISTransform[0, 1].ToString());
        rotateElement.SetAttribute("r02", moveToRUISTransform[0, 2].ToString());
        rotateElement.SetAttribute("r10", moveToRUISTransform[1, 0].ToString());
        rotateElement.SetAttribute("r11", moveToRUISTransform[1, 1].ToString());
        rotateElement.SetAttribute("r12", moveToRUISTransform[1, 2].ToString());
        rotateElement.SetAttribute("r20", moveToRUISTransform[2, 0].ToString());
        rotateElement.SetAttribute("r21", moveToRUISTransform[2, 1].ToString());
        rotateElement.SetAttribute("r22", moveToRUISTransform[2, 2].ToString());

        m2kElement.AppendChild(rotateElement);

        XmlElement kinectFloorNormalElement = xmlDoc.CreateElement("kinectFloorNormal");
        kinectFloorNormalElement.SetAttribute("x", kinectFloorNormal.x.ToString());
        kinectFloorNormalElement.SetAttribute("y", kinectFloorNormal.y.ToString());
        kinectFloorNormalElement.SetAttribute("z", kinectFloorNormal.z.ToString());

        m2kElement.AppendChild(kinectFloorNormalElement);

        XmlElement kinectDistanceFromFloorElement = xmlDoc.CreateElement("kinectDistanceFromFloor");
        kinectDistanceFromFloorElement.SetAttribute("value", kinectDistanceFromFloor.ToString());

        m2kElement.AppendChild(kinectDistanceFromFloorElement);

        FileStream xmlFileStream = File.Open(filename, FileMode.Create);
        StreamWriter streamWriter = new StreamWriter(xmlFileStream);
        xmlDoc.Save(streamWriter);
        streamWriter.Flush();
        streamWriter.Close();
        xmlFileStream.Close();
    }

    

    public void ResetTransforms()
    {
        SetMoveToKinectTransforms(Matrix4x4.identity, Matrix4x4.identity);
        kinectDistanceFromFloor = 0;
    }

    public void SetMoveToKinectTransforms(Matrix4x4 transformMatrix, Matrix4x4 rotationMatrix)
    {
        moveToRUISTransform = transformMatrix;
        moveToKinectRotation = MathUtil.QuaternionFromMatrix(rotationMatrix);
    }

    public void SetKinectFloorNormal(Vector3 normal)
    {
        kinectFloorNormal = normal.normalized;
        kinectFloorRotator = Quaternion.identity;
        kinectFloorRotator.SetFromToRotation(Vector3.up, kinectFloorNormal);
    }

    public void ResetKinectFloorNormal()
    {
        kinectFloorNormal = Vector3.up;
        kinectFloorRotator = Quaternion.identity;
    }

    public void SetKinectDistanceFromFloor(float distance)
    {
        kinectDistanceFromFloor = distance;
    }
	
	public float GetKinectDistanceFromFloor()
	{
		return kinectDistanceFromFloor;
	}

    public void ResetKinectDistanceFromFloor()
    {
        kinectDistanceFromFloor = 0;
    }

    public Vector3 ConvertMovePosition(Vector3 position)
    {
        //flip the z coordinate to get into unity's coordinate system
        Vector3 newPosition = new Vector3(position.x, position.y, -position.z);

        newPosition *= moveToUnityScale;

		if (applyMoveToKinect)
        	newPosition = moveToRUISTransform.MultiplyPoint3x4(newPosition);

        newPosition = Quaternion.Euler(0, yawOffset, 0) * newPosition;

        if (setKinectOriginToFloor)
        {
            newPosition.y += kinectDistanceFromFloor;
        }
            
        newPosition += positionOffset;

        return newPosition;
    }
	
	// TUUKKA:
	public Vector3 ConvertMoveVelocity(Vector3 velocity)
    {
        //flip the z coordinate to get into unity's coordinate system
        Vector3 newVelocity = new Vector3(velocity.x, velocity.y, -velocity.z);

        newVelocity *= moveToUnityScale;
		
		if (applyMoveToKinect)
        	newVelocity = moveToRUISTransform.MultiplyPoint3x4(newVelocity);

        newVelocity = Quaternion.Euler(0, yawOffset, 0) * newVelocity;

        return newVelocity;
    }

    public Quaternion ConvertMoveRotation(Quaternion rotation)
    {
        Quaternion newRotation = rotation;

        //this turns the quaternion into the correct direction
        newRotation.x = -newRotation.x;
        newRotation.y = -newRotation.y;

        if (applyMoveToKinect)
        {
            newRotation = moveToKinectRotation * newRotation;
        }

        newRotation = Quaternion.Euler(0, yawOffset, 0) * newRotation;

        /*if (applyKinectToRUIS)
        {
            rotation *= kinectYaw;
        }*/

        return newRotation;
    }

    public Vector3 ConvertMoveAngularVelocity(Vector3 angularVelocity)
    {
        Vector3 newVelocity = angularVelocity;
        newVelocity.x = -newVelocity.x;
        newVelocity.y = -newVelocity.y;
		if (applyMoveToKinect)
	        newVelocity = moveToKinectRotation * newVelocity;
        newVelocity = Quaternion.Euler(0, yawOffset, 0) * newVelocity;
        return newVelocity;
    }

//    public Vector3 ConvertKinectPosition(OpenNI.Point3D position)
//    {
//        //we have to flip the z axis to get into unity's coordinate system
//        Vector3 newPosition = Vector3.zero;
//        newPosition.x = position.X;
//        newPosition.y = position.Y;
//        newPosition.z = -position.Z;
//
//        newPosition = kinectToUnityScale * (kinectFloorRotator * newPosition);
//
//        return newPosition;
//    }

	public Vector3 ConvertKinectPosition(OpenNI.Point3D position)
	{
		//we have to flip the z axis to get into unity's coordinate system
		Vector3 newPosition = Vector3.zero;
		newPosition.x = position.X;
		newPosition.y = position.Y;
		newPosition.z = -position.Z;
		
		newPosition = kinectToUnityScale * (Quaternion.Euler(0, yawOffset, 0) * kinectFloorRotator * newPosition);
		
		if (setKinectOriginToFloor)
		{
			newPosition.y += kinectDistanceFromFloor;
		}
		
		newPosition += positionOffset;
		
		return newPosition;
	}
    
    public Quaternion ConvertKinectRotation(OpenNI.SkeletonJointOrientation rotation)
    {
        Vector3 up = new Vector3(rotation.Y1, rotation.Y2, rotation.Y3);
        Vector3 forward = new Vector3(rotation.Z1, rotation.Z2, rotation.Z3);

        if (up == Vector3.zero || forward == Vector3.zero) return Quaternion.identity;

        Quaternion newRotation = Quaternion.LookRotation(forward, up);

        newRotation.x = -newRotation.x;
        newRotation.y = -newRotation.y;

		newRotation = Quaternion.Euler(0, yawOffset, 0) * kinectFloorRotator * newRotation;
		//newRotation = Quaternion.Euler(0, yawOffset, 0) * newRotation;
        //if (applyKinectToRUIS) result *= kinectYaw;

        return newRotation;
    }
}

