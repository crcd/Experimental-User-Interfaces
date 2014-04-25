using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]  

public class IKCtrl : MonoBehaviour {
	
	protected Animator animator;
	
	public bool leftIKActive = false;
	public Transform leftHandObj = null;
	public Vector3 leftPositionOffset = new Vector3 (0,0,0);
	public Vector3 leftRotationOffset = new Vector3 (0, 0, 0);
	
	public bool rightIKActive = false;
	public Transform rightHandObj = null;
	public Vector3 rightPositionOffset = new Vector3 (0,0,0);
	public Vector3 rightRotationOffset = new Vector3 (0, 0, 0);
	
	// Animate IK translations
	private int animStep = 0;
	public int animLength = 60; // 1s
	private float ikWeight = 1.0f;

	
	void Start () 
	{
		animator = GetComponent<Animator>();
	}
	
	//a callback for calculating IK
	void OnAnimatorIK()
	{
		if(animator) {
			
			if (leftIKActive || rightIKActive) {
				++animStep;
			} else {
				--animStep;
			}
			if (animStep > animLength) {
				animStep = animLength;
			}
			if (animStep < 1) {
				animStep = 1;
			}
			
			ikWeight = (animStep / (float)animLength);
				
				//if the IK is active, set the position and rotation directly to the goal. 

			//set the position and the rotation of the left hand where the external object is
			if(leftHandObj != null) {
				animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandObj.position + leftPositionOffset);
				animator.SetIKRotation(AvatarIKGoal.LeftHand,leftHandObj.rotation * Quaternion.Euler(leftRotationOffset));
			}  

			if(leftIKActive) {
				
				//weight = 1.0 for the right hand means position and rotation will be at the IK goal (the place the character wants to grab)
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,ikWeight);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,ikWeight);                 
				
			}
			
			//if the IK is not active, set the position and rotation of the hand back to the original position
			else {          
				animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,ikWeight);
				animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,ikWeight);             
			}

			//set the position and the rotation of the right hand where the external object is
			if(rightHandObj != null) {
				animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandObj.position + rightPositionOffset);
				animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandObj.rotation * Quaternion.Euler(rightRotationOffset));
			}   
			
			//if the IK is active, set the position and rotation directly to the goal. 
			if(rightIKActive) {
				
				//weight = 1.0 for the right hand means position and rotation will be at the IK goal (the place the character wants to grab)
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand,ikWeight);
				animator.SetIKRotationWeight(AvatarIKGoal.RightHand,ikWeight);                
				
			}
			
			//if the IK is not active, set the position and rotation of the hand back to the original position
			else {          
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand,ikWeight);
				animator.SetIKRotationWeight(AvatarIKGoal.RightHand,ikWeight);
			}
			
			
		}
	}     
}