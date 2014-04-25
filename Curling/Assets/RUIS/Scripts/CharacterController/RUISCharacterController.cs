/*****************************************************************************

Content    :   A Script to handle controlling a rigidbody character using Kinect and some traditional input method
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RUISCharacterController : MonoBehaviour
{
    public enum CharacterPivotType
    {
        KinectHead,
        KinectTorso,
        MoveController
    }

    public CharacterPivotType characterPivotType = CharacterPivotType.KinectTorso;

    public int kinectPlayerId;
    public int moveControllerId;
    public Transform characterPivot;
	public Vector3 psmoveOffset = Vector3.zero;

    public bool ignorePitchAndRoll = true;

    private RUISInputManager inputManager;
    private RUISSkeletonManager skeletonManager;

    public LayerMask groundLayers;
    public float groundedErrorTweaker = 0.05f;

    public bool grounded { get; private set; }
    public bool colliding { get; private set; }
	private bool wasColliding = false;
	
	Vector3 raycastPosition;
	float distanceToRaycast;
	
	RaycastHit hitInfo;
	private bool tempGrounded = false;
	private bool rayIntersected = false;
	
    private RUISCharacterStabilizingCollider stabilizingCollider;
	
	public bool dynamicFriction = true;
	public PhysicMaterial dynamicMaterial;
	private PhysicMaterial originalMaterial;
	private Collider colliderComponent;
    public float lastJumpTime { get; set; }
	
	public bool feetAlsoAffectGrounding = true;
	List<Transform> bodyParts = new List<Transform>(2);
	RUISSkeletonController skeletonController;

    void Awake()
    {
        inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        stabilizingCollider = GetComponentInChildren<RUISCharacterStabilizingCollider>();
		lastJumpTime = 0;
    }
	
    void Start()
    {
		colliding = false;
		grounded = false;
	
		skeletonController = gameObject.GetComponentInChildren<RUISSkeletonController>();
		if(skeletonController)
		{
			bodyParts.Add(skeletonController.leftFoot);
			bodyParts.Add(skeletonController.rightFoot);
			kinectPlayerId = skeletonController.playerId;
		}
		else
		{
			Debug.LogError(  "RUISCharacterController script in game object '" + gameObject.name 
			               + "' did not find RUISSkeletonController component from it's child objects!");
		}

		if(stabilizingCollider)
		{	
			colliderComponent = stabilizingCollider.gameObject.collider;
			if(colliderComponent)
			{
				if(    characterPivotType == CharacterPivotType.KinectHead
				    || characterPivotType == CharacterPivotType.KinectTorso)
				{
					RUISCoordinateSystem coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
					if(coordinateSystem && inputManager.enableKinect && !coordinateSystem.setKinectOriginToFloor)
						Debug.LogWarning("It is best to enable 'setKinectOriginToFloor' from RUISCoordinateSystem " +
						                 "when using Kinect and RUISCharacterController script.");
				}

				if(colliderComponent.material)
					originalMaterial = colliderComponent.material;
				else
				{
					colliderComponent.material = new PhysicMaterial();
					originalMaterial = colliderComponent.material;
				}
				
				if(dynamicMaterial == null)
				{
					dynamicMaterial = new PhysicMaterial();
					
					dynamicMaterial.dynamicFriction = 0;
					dynamicMaterial.staticFriction = 0;
					dynamicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
					
					if(colliderComponent.material)
					{
						dynamicMaterial.bounceCombine = originalMaterial.bounceCombine;
						dynamicMaterial.bounciness = originalMaterial.bounciness;
						dynamicMaterial.staticFriction2 = originalMaterial.staticFriction2;
						dynamicMaterial.dynamicFriction2 = originalMaterial.dynamicFriction2;
						dynamicMaterial.frictionDirection2 = originalMaterial.frictionDirection2;
					}
				}
			}
		}
		if((   characterPivotType == CharacterPivotType.KinectHead
		    || characterPivotType == CharacterPivotType.KinectTorso)
		    && (skeletonController && skeletonController.playerId != kinectPlayerId))
			Debug.LogError(  "The 'Kinect Player Id' variable in RUISCharacterController script in gameObject '" + gameObject.name
			               + "is different from the Kinect Player Id of the RUISSkeletonController script (located in child "
			               + "object '" + skeletonController.gameObject.name + "). Make sure that these two values are "
			               + "the same.");

	}
	
    void Update()
    {
		// Check whether character collider (aka stabilizingCollider) is grounded
        raycastPosition = stabilizingCollider ? stabilizingCollider.transform.position : transform.position;

        distanceToRaycast = stabilizingCollider ? stabilizingCollider.colliderHeight / 2 : 1.5f;
        distanceToRaycast += groundedErrorTweaker;
		distanceToRaycast = Mathf.Max(distanceToRaycast, float.Epsilon);
		
        tempGrounded = Physics.Raycast(raycastPosition, -transform.up, distanceToRaycast, groundLayers.value);
		
		// Check if feet are grounded
		if(!tempGrounded && feetAlsoAffectGrounding)
		{
			foreach(Transform bodyPart in bodyParts)
			{
	            if(bodyPart && bodyPart.collider)
	            {
					raycastPosition = bodyPart.collider.bounds.center;
					distanceToRaycast = bodyPart.collider.bounds.extents.y + groundedErrorTweaker;
					rayIntersected = Physics.Raycast(raycastPosition, -transform.up, out hitInfo, 
													 distanceToRaycast, groundLayers.value		  );
					
					if(rayIntersected && hitInfo.rigidbody)
					{
						if(!hitInfo.rigidbody.isKinematic)
						{
							tempGrounded = true;
							break;
						}
					}
				}
			}
		}
		
        grounded = tempGrounded;
    }
	
	void FixedUpdate()
	{
		if(wasColliding)
			colliding = true;
		else
			colliding = false;
		wasColliding = false;
		
		if(stabilizingCollider)
		{
			if(dynamicFriction)
			{
				colliderComponent = stabilizingCollider.gameObject.collider;
				if(colliderComponent)
				{
					if(colliderComponent.material)
					{
						
						if(grounded && (Time.fixedTime - lastJumpTime) > 1)
						{
							colliderComponent.material = originalMaterial;
						}
						else
						{
							colliderComponent.material = dynamicMaterial; 
						}
					}
				}
			}
		}
	}

    public void RotateAroundCharacterPivot(Vector3 eulerRotation)
    {
        Vector3 pivotPosition = GetPivotPositionInTrackerCoordinates();
        if (pivotPosition == Vector3.zero)
        {
            rigidbody.MoveRotation(Quaternion.Euler(eulerRotation) * transform.rotation);
            return;
        }

        pivotPosition = transform.TransformPoint(pivotPosition);
        //Debug.Log("pivotPosition: " + pivotPosition);
        //Debug.DrawLine(pivotPosition, transform.position, Color.blue);

        Vector3 positionDiff = pivotPosition - transform.position;
        //Debug.Log("old: " + positionDiff);
        //positionDiff.y = 0;
        //Debug.DrawLine(pivotPosition - positionDiff, pivotPosition, Color.red);

        positionDiff = Quaternion.Euler(eulerRotation) * positionDiff;
        //Debug.DrawLine(transform.position, pivotPosition, Color.red);
        //Debug.DrawLine(pivotPosition - positionDiff, pivotPosition, Color.green);

        //Debug.Log("new: " + positionDiff);
        Vector3 newPosition = pivotPosition - positionDiff;
        //Debug.DrawLine(transform.position, newPosition, Color.yellow);
        rigidbody.MovePosition(newPosition);
        rigidbody.MoveRotation(Quaternion.Euler(eulerRotation) * transform.rotation);
    }

    public Vector3 TransformDirection(Vector3 directionInCharacterCoordinates)
    {
        Vector3 characterForward = Vector3.forward;

        
        switch (characterPivotType)
        {
            case CharacterPivotType.KinectHead:
				if(skeletonManager != null && skeletonManager.skeletons[kinectPlayerId] != null)
					characterForward = skeletonManager.skeletons[kinectPlayerId].head.rotation * Vector3.forward;
				else 
					characterForward = Vector3.forward;
                break;
			case CharacterPivotType.KinectTorso:
				if(skeletonManager != null && skeletonManager.skeletons[kinectPlayerId] != null)
					characterForward = skeletonManager.skeletons[kinectPlayerId].torso.rotation * Vector3.forward;
				else 
					characterForward = Vector3.forward;
                break;
            case CharacterPivotType.MoveController:
			{
				RUISPSMoveWand psmove = inputManager.GetMoveWand(moveControllerId);
				if(psmove != null)
	                characterForward = psmove.localRotation * Vector3.forward;
			}
                break;
        }

        if (ignorePitchAndRoll)
        {
            characterForward.y = 0;
            characterForward.Normalize();
        }

        characterForward = transform.TransformDirection(characterForward);


        return Quaternion.LookRotation(characterForward, transform.up) * directionInCharacterCoordinates;
    }

    private Vector3 GetPivotPositionInTrackerCoordinates()
    {
        switch (characterPivotType)
        {
            case CharacterPivotType.KinectHead:
			{
				if(skeletonManager && skeletonManager.skeletons[kinectPlayerId] != null)
                	return skeletonManager.skeletons[kinectPlayerId].head.position;
				break;
			}
            case CharacterPivotType.KinectTorso:
			{
				if(skeletonManager && skeletonManager.skeletons[kinectPlayerId] != null)
	                return skeletonManager.skeletons[kinectPlayerId].torso.position;
				break;
			}
            case CharacterPivotType.MoveController:
			{
				if(inputManager.GetMoveWand(moveControllerId))
	                return inputManager.GetMoveWand(moveControllerId).handlePosition;
				break;
			}
        }

        return Vector3.zero;
    }

    public void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        Vector3 pivotPosition = GetPivotPositionInTrackerCoordinates();
        pivotPosition = transform.TransformPoint(pivotPosition);

        Gizmos.DrawLine(transform.position, pivotPosition);
    }
	
	
	void OnCollisionStay(Collision other)
	{
		// Check if collider belongs to groundLayers
		//if((groundLayers.value & (1 << other.gameObject.layer)) > 0)
		//{
		wasColliding = true;
		//Debug.LogError(other.gameObject.name);
		//}
	}
}
