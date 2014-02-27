/*****************************************************************************

Content    :   The functionality for selectable objects, just add this to an object along with a collider to make it selectable
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSelectable")]
public class RUISSelectable : MonoBehaviour {

    private bool rigidbodyWasKinematic;
    private RUISWandSelector selector;
    private RUISWandSelector rotator;

    private RUISPSMoveWand selectorWand;
    private RUISPSMoveWand rotatorWand;

    public bool isSelected { get { return selector != null; } }

    private Vector3 positionAtSelection;
    private Quaternion rotationAtSelection;
    private Vector3 selectorPositionAtSelection;
    private Quaternion selectorRotationAtSelection;
    private float distanceFromSelectionRayOrigin;
    public float DistanceFromSelectionRayOrigin
    {
        get
        {
            return distanceFromSelectionRayOrigin;
        }
    }

    public bool clampToCertainDistance = false;
    public float distanceToClampTo = 1.0f;
    
    //for highlights
    public Material highlightMaterial;
    public Material selectionMaterial;

    public bool maintainMomentumAfterRelease = true;
	
	public bool continuousCollisionDetectionWhenSelected = true;
	private CollisionDetectionMode oldCollisionMode;
	private bool switchToOldCollisionMode = false;
	private bool switchToContinuousCollisionMode = false;

    private Vector3 latestVelocity = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    private List<Vector3> velocityBuffer;

    private bool transformHasBeenUpdated = false;

    public void Awake()
    {
        velocityBuffer = new List<Vector3>();
		if(rigidbody)
			oldCollisionMode = rigidbody.collisionDetectionMode;
    }

    public void Update()
    {
        if (transformHasBeenUpdated)
        {
            latestVelocity = (transform.position - lastPosition) 
								/ Mathf.Max(Time.deltaTime, Time.fixedDeltaTime);
            lastPosition = transform.position;

            velocityBuffer.Add(latestVelocity);
            LimitBufferSize(velocityBuffer, 2);

            transformHasBeenUpdated = false;
        }
    }

    public void FixedUpdate()
    {
		if(switchToContinuousCollisionMode)
		{
			oldCollisionMode = rigidbody.collisionDetectionMode;
			rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			switchToContinuousCollisionMode = false;
		}
		if(switchToOldCollisionMode)
		{
			rigidbody.collisionDetectionMode = oldCollisionMode;
			switchToOldCollisionMode = false;
		}
        UpdateTransform(true);
        transformHasBeenUpdated = true;
    }

    public virtual void OnSelection(RUISWandSelector selector)
    {
        this.selectorWand = GameObject.Find("PSMoveWand-Translate").GetComponent<RUISPSMoveWand>();
        this.rotatorWand = GameObject.Find("PSMoveWand-Rotate").GetComponent<RUISPSMoveWand>();
        this.selector = selector;
        this.rotator = GameObject.Find("PSMoveWand-Rotate").GetComponent<RUISWandSelector>();
        Debug.Log(this.rotator);

        positionAtSelection = transform.position;
        rotationAtSelection = transform.rotation;
        selectorPositionAtSelection = selector.transform.position;
        selectorRotationAtSelection = selector.transform.rotation;
        distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude;

        lastPosition = transform.position;

        if (rigidbody)
        {
			if(continuousCollisionDetectionWhenSelected)
			{
				switchToContinuousCollisionMode = true;
			}
            rigidbodyWasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
        }

        if (selectionMaterial != null)
            AddMaterialToEverything(selectionMaterial);

        UpdateTransform(false);
    }


    public virtual void OnSelectionEnd()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = rigidbodyWasKinematic;
			if(continuousCollisionDetectionWhenSelected)
			{
				switchToOldCollisionMode = true;
			}
        }

        if (maintainMomentumAfterRelease && rigidbody && !rigidbody.isKinematic)
        {
            rigidbody.AddForce(AverageBufferContent(velocityBuffer), ForceMode.VelocityChange);
			if(selector) // Put this if-clause here just in case because once received NullReferenceException
	            rigidbody.AddTorque(Mathf.Deg2Rad * selector.angularVelocity, ForceMode.VelocityChange);
        }

        if(selectionMaterial != null)
            RemoveMaterialFromEverything();

        this.selector = null;
    }

    public virtual void OnSelectionHighlight()
    {
        if(highlightMaterial != null)
            AddMaterialToEverything(highlightMaterial);
    }

    public virtual void OnSelectionHighlightEnd()
    {
        if(highlightMaterial != null)
            RemoveMaterialFromEverything();
    }

    protected virtual void UpdateTransform(bool safePhysics)
    {
        if (!isSelected) return;

        Vector3 newPosition = transform.position;
        Quaternion newRotation = transform.rotation;

        switch (selector.positionSelectionGrabType)
        {
            case RUISWandSelector.SelectionGrabType.SnapToWand:
                newPosition = selector.transform.position;
                break;
            case RUISWandSelector.SelectionGrabType.RelativeToWand:
                Vector3 selectorPositionChange = selector.transform.position - selectorPositionAtSelection;
                newPosition = positionAtSelection + selectorPositionChange;
                break;
            case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
                float clampDistance = distanceFromSelectionRayOrigin;
                if (clampToCertainDistance) clampDistance = distanceToClampTo;
                newPosition = selector.selectionRay.origin + clampDistance * selector.selectionRay.direction;
                break;
            case RUISWandSelector.SelectionGrabType.DoNotGrab:
                break;
        }

        if (rotator) {

            switch (rotator.rotationSelectionGrabType)
            {
                case RUISWandSelector.SelectionGrabType.SnapToWand:
                    newRotation = rotator.transform.rotation;
                    break;
                case RUISWandSelector.SelectionGrabType.RelativeToWand:
                    newRotation = rotationAtSelection;
                    // Tuukka: 
                    Quaternion selectorRotationChange = Quaternion.Inverse(selectorRotationAtSelection) * rotationAtSelection;
                    newRotation = rotator.transform.rotation * selectorRotationChange;
                    break;
                case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
                    newRotation = Quaternion.LookRotation(rotator.selectionRay.direction);
                    break;
                case RUISWandSelector.SelectionGrabType.DoNotGrab:
                    break;
            }

            if (rotatorWand.squareButtonDown) {
                transform.localScale = transform.localScale * 1.05f;
            }
            if (rotatorWand.triangleButtonDown) {
                transform.localScale = transform.localScale * 0.95f;
            }
        }

        if (rigidbody && safePhysics)
        {
            rigidbody.MovePosition(newPosition);
            rigidbody.MoveRotation(newRotation);
        }
        else
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
        if (selectorWand.squareButtonDown) {
            distanceFromSelectionRayOrigin += 0.25f;
            //transform.position = transform.position + selector.transform.forward;
        }
        else if (selectorWand.triangleButtonDown) {
            distanceFromSelectionRayOrigin -= 0.25f;
            //transform.position = transform.position - selector.transform.forward;
        }

    }


    private void LimitBufferSize(List<Vector3> buffer, int maxSize)
    {
        while (buffer.Count >= maxSize)
        {
            buffer.RemoveAt(0);
        }
    }

    private Vector3 AverageBufferContent(List<Vector3> buffer)
    {
        if (buffer.Count == 0) return Vector3.zero;

        Vector3 averagedContent = new Vector3();
        foreach (Vector3 v in buffer)
        {
            averagedContent += v;
        }

        averagedContent = averagedContent / buffer.Count;

        return averagedContent;
    }

    private void AddMaterial(Material m, Renderer r)
    {
        if (	m == null || r == null || r.GetType() == typeof(ParticleRenderer) 
			||  r.GetType() == typeof(ParticleSystemRenderer))
			return;

        Material[] newMaterials = new Material[r.materials.Length + 1];
        for (int i = 0; i < r.materials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }

        newMaterials[newMaterials.Length - 1] = m;
        r.materials = newMaterials;
    }

    private void RemoveMaterial(Renderer r)
    {
        if (	r == null || r.GetType() == typeof(ParticleRenderer) 
			||  r.GetType() == typeof(ParticleSystemRenderer) || r.materials.Length == 0)
			return;

        Material[] newMaterials = new Material[r.materials.Length - 1];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }
        r.materials = newMaterials;
    }

    private void AddMaterialToEverything(Material m)
    {
        AddMaterial(m, renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            AddMaterial(m, childRenderer);
        }
    }

    private void RemoveMaterialFromEverything()
    {
        RemoveMaterial(renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            RemoveMaterial(childRenderer);
        }
    }
}
