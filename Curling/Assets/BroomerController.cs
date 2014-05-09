﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISCharacterLocomotion))]
public class BroomerController : MonoBehaviour {
	private Rigidbody stoneBody;
	private Rigidbody broomerBody;
	public Vector3 stoneOffset = new Vector3(-1,0,3);
	private Vector3 broomerStartingPos;
	private bool broomerSliding;
	private IKCtrl ikCtrl;
    private BroomController broomController;

//	private Vector3 targetVelocity;
//	private Vector3 velocity;
//	private Vector3 velocityChange;

	private RUISCharacterLocomotion charLocomotion;
	
	
	void Start () {
		this.broomerBody = rigidbody;
		this.broomerStartingPos = rigidbody.position;
		this.ikCtrl = GameObject.Find ("baseMaleBroomer").GetComponent<IKCtrl> ();
		this.charLocomotion = gameObject.GetComponent<RUISCharacterLocomotion> ();
        this.broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
	}
	
	public void setStone (Rigidbody stoneBody) {
		this.stoneBody = stoneBody;
	}
	
	private void followStone () {
        this.charLocomotion.SetFixedTargetVelocity (new Vector3(
            this.stoneBody.velocity.x,
            0,
            this.stoneBody.velocity.z
        ));

//		targetVelocity = this.stoneBody.velocity;
//		velocity = rigidbody.velocity;
//		velocityChange = (targetVelocity - velocity);
//		velocityChange.y = 0;
		//velocityChange = Vector3.ClampMagnitude(velocityChange, Time.fixedDeltaTime * maxVelocityChange);

//		broomerBody.AddForce(velocityChange, ForceMode.VelocityChange);

		
//		Vector3 newPosition = new Vector3 (
//			this.stoneBody.position.x + this.stoneOffset.x,
//			this.broomerBody.position.y,
//			this.stoneBody.position.z + this.stoneOffset.z
//			);
//
//		this.broomerBody.position = newPosition;
//		this.broomerBody.velocity = new Vector3 (0, 0, 0);
	}
	
	public void resetToStartPosition () {
		this.broomerBody.position = this.broomerStartingPos;
		this.broomerBody.velocity = new Vector3 (0, 0, 0);
		this.broomerBody.angularVelocity = new Vector3 (0, 0, 0);
	}

    bool isStoneMoving() {
        //return Vector3.Distance (this.stoneBody.velocity, new Vector3 (0, 0, 0)) >= 0.05f;
		return this.stoneBody.velocity.magnitude >= 0.05f;
    }
	
	void FixedUpdate () {
		if (this.stoneBody) {
            if (isStoneMoving ()) {
                this.followStone ();
				// Update when PS Move is connected
				//this.broomController.brooming = true;
			} else {
				this.charLocomotion.SetFixedTargetVelocity (new Vector3(0,0,0));
                //this.broomController.brooming = false;
			}
		}
	}
}
