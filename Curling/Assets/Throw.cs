﻿using UnityEngine;
using System.Collections;

public abstract class Throw : MonoBehaviour {
    public Vector3 slidingVelocityFactor;
    public Vector3 releaseVelocityFactor;
    public Vector3 handlingFactor;
    public float initialDistanceToMove;
    public float distanceToMove;
    public Time throwTime;
    public float minControllerThrowVelocity;
    protected ThrowerController throwerController;
    protected GameLogic gameLogic;
    protected Vector3 startingMovePosition;
    protected float throwStartTime;
    protected bool footInStartingPosition;
    protected bool initialized;
    private Vector3 spawnPosition;
    private bool minVelocityReached;

    protected void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
        this.gameLogic = GameObject.Find ("GameLogic").GetComponent<GameLogic> ();
    }

    abstract protected bool wasReleased ();

    abstract protected bool isPressed ();

    abstract protected bool spawnButtonPressed ();

    abstract protected float getAngularVelocity ();

    abstract protected float getRotation ();

    abstract protected Vector3 getControllerVelocity ();

    abstract protected Vector3 getControllerPosition ();

    bool isThrowStarted () {
        return this.throwStartTime > 0;
    }

    bool isInitialThrowLineCrossed () {
        return getControllerPosition ().z - this.startingMovePosition.z > this.initialDistanceToMove;
    }

    bool isEndlineCrossed () {
        return getControllerPosition ().z - this.startingMovePosition.z > this.distanceToMove;
    }

    Vector3 getReleaseVelocity () {
        return Vector3.Scale (this.getControllerVelocity (), this.releaseVelocityFactor);
    }

    void handleStoneObject () {
        this.throwerController.setStoneRotation (this.getRotation ());
        Vector3 offset = getControllerPosition () - this.spawnPosition;
        this.throwerController.sawStone (Vector3.Scale (offset, this.handlingFactor));
    }

    void handleSpawning () {
        if (spawnButtonPressed ()) {
            this.gameLogic.startNewThrow ();
            this.footInStartingPosition = true;
            this.initialized = false;
            this.spawnPosition = getControllerPosition ();
        }
    }

    void initializeThrow () {
        this.footInStartingPosition = true;
        this.startingMovePosition = getControllerPosition ();
        this.initialized = true;
        this.throwStartTime = 0;
        this.minVelocityReached = false;
    }

    void startThrust () {
        this.startingMovePosition = getControllerPosition ();
        this.throwStartTime = Time.time;
    }

    void endThrust () {
        this.footInStartingPosition = false;
        float timeDelta = Time.time - this.throwStartTime;
        float xDelta = getControllerPosition ().x - this.startingMovePosition.x;
        Vector3 vel = new Vector3 (xDelta / timeDelta, 0, distanceToMove / timeDelta);
        //this.throwerController.startSliding (Vector3.Scale (vel, this.slidingVelocityFactor));
    }

    void releaseFoot() {
        this.footInStartingPosition = false;
        this.minVelocityReached = false;
        this.throwerController.startSliding ();
    }

    bool isControllerOverMinVelocity() {
        return getControllerVelocity ().z > this.minControllerThrowVelocity;
    }

    void handleThrustingVelocity () {
        if (!isPressed())
            return;
        if (!this.minVelocityReached) {
            if (isControllerOverMinVelocity ())
                minVelocityReached = true;
        }
        else {
            if (isControllerOverMinVelocity ())
                this.throwerController.addSlidingForce (
                    Vector3.Scale(
                        this.getControllerVelocity(),
                        this.slidingVelocityFactor
                    )

                );
            else
                releaseFoot();
        }
            
    }

    void throwStone () {
        //Handle if foot still in startingpos
        //if (this.footInStartingPosition)
        //    endThrust ();
        this.throwerController.throwStone (this.getReleaseVelocity (), this.getAngularVelocity ());
        this.initialized = false;
    }

    void handleThrow () {
        if (this.isPressed () && !this.initialized)
            initializeThrow ();
        else if (this.wasReleased () || (!this.isPressed () && this.initialized))
            throwStone ();

        if (this.initialized && this.footInStartingPosition)
            handleThrustingVelocity ();
    }

    protected void FixedUpdate () {
        if (this.throwerController) {
            this.handleStoneObject ();
            this.handleSpawning ();
            this.handleThrow ();
        }
    }
}
