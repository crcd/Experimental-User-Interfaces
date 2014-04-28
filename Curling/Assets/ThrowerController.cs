﻿using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    public Vector3 stoneOffsetConfig;
    public Vector3 sawingStartPosition;
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    private Vector3 stoneOffset;
    private Vector3 throwerStartingPos;
    private bool throwerSliding;
    private IKCtrl ikCtrl;
    private BroomController broomController;

    void Start () {
        this.throwerBody = rigidbody;
        this.throwerStartingPos = rigidbody.position;
        this.ikCtrl = GameObject.Find ("baseMaleThrower").GetComponent<IKCtrl> ();
        this.broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
    }

    public void setStone (Rigidbody stoneBody) {
        this.stoneBody = stoneBody;
        this.stoneOffset = this.stoneOffsetConfig;
        this.ikCtrl.rightHandObj = stoneBody.transform;
        this.ikCtrl.rightIKActive = true;
    }

    public void throwStone (Vector3 additionalVelocity, float yRotation) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, yRotation, 0);
            this.stoneBody.velocity = this.throwerBody.velocity + additionalVelocity;
            this.stoneBody = null;
        }
        if (throwerSliding)
            throwerSliding = false;
        // this.ikCtrl.rightHandObj = null; // IKCtrl has animation
        this.ikCtrl.rightIKActive = false;

        // Just for testing
        this.broomController.brooming = true;


    }

    public void startSliding (Vector3 force) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, 0, 0);
        }
        this.throwerSliding = true;
        this.throwerBody.AddForce (force, ForceMode.VelocityChange);
    }

    public void resetToStartPosition () {
        this.throwerBody.position = this.throwerStartingPos;
        this.throwerBody.velocity = new Vector3 (0, 0, 0);
        this.throwerBody.angularVelocity = new Vector3 (0, 0, 0);

        // Just for testing
        this.broomController.brooming = false;
		
    }

    public void setStoneRotation (float stoneRotation) {
        if (this.stoneBody) {
            this.stoneBody.transform.eulerAngles = new Vector3 (0, stoneRotation, 0);
        }
    }

    public void sawStone (Vector3 controllerPosition) {
        if (this.stoneBody != null && this.sawingStartPosition != null) {
            Vector3 newPosition = new Vector3 (
                                      this.sawingStartPosition.x - controllerPosition.x,
                                      0,
                                      this.sawingStartPosition.z - controllerPosition.z
                                  );
            this.stoneOffset = this.stoneOffsetConfig - newPosition;
        }
    }

    private void keepStonePositionInHand () {
        //A bit of a workaround to give stone velocity for the sound
        if (this.throwerBody.velocity.z > 0.01) {
            this.stoneBody.velocity = this.throwerBody.velocity;
        }
        Vector3 newPosition = new Vector3 (
                                  this.throwerBody.position.x + stoneOffset.x,
                                  this.stoneBody.position.y,
                                  this.throwerBody.position.z + stoneOffset.z
                              );
        this.stoneBody.position = newPosition;
    }

    void Update () {
        if (this.stoneBody) {
            this.keepStonePositionInHand ();
        }
    }
}
