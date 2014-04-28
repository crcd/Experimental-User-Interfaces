﻿using UnityEngine;
using System.Collections;

public abstract class Throw : MonoBehaviour {
    public Vector3 slidingVelocityFactor;
    public Vector3 releaseVelocityFactor;
    public float initialDistanceToMove;
    public float distanceToMove;
    protected ThrowerController throwerController;
    protected StoneSpawner stoneSpawner;
    protected Vector3 startingMovePosition;
    protected float throwStartTime;
    protected bool footInStartingPosition;
    protected bool initialized;

    void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
        this.stoneSpawner = GameObject.Find ("GameLogic").GetComponent<StoneSpawner> ();
    }

    abstract protected bool wasReleased ();

    abstract protected bool isPressed ();

    abstract protected bool spawnButtonPressed ();

    abstract protected float getAngularVelocity ();

    abstract protected float getRotation ();

    abstract protected Vector3 getControllerVelocity ();

    abstract protected Vector3 getControllerPosition ();

    bool isThrowStarted() {
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
        if (!this.throwerController.isSawingPositionSet) {
            this.throwerController.isSawingPositionSet = true;
            this.throwerController.sawingStartPosition = getControllerPosition ();
        } else {
            this.throwerController.sawStone (getControllerPosition ());
        }
    }

    void handleSpawning () {
        if (spawnButtonPressed ()) {
            this.stoneSpawner.spawnNewStone ();
            this.footInStartingPosition = true;
            this.initialized = false;
        }
    }

    void initializeThrow () {
        this.footInStartingPosition = true;
        this.startingMovePosition = getControllerPosition ();
        this.initialized = true;
        this.throwStartTime = 0;
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
        this.throwerController.startSliding (Vector3.Scale (vel, this.slidingVelocityFactor));
    }

    void handleThrustingVelocity () {
        if (this.isInitialThrowLineCrossed () && !isThrowStarted())
            startThrust ();
        else if (this.isEndlineCrossed ())
          endThrust ();
    }

    void throwStone () {
        //Handle if foot still in startingpos
        if (this.footInStartingPosition)
            endThrust ();
        this.throwerController.throwStone (this.getReleaseVelocity (), this.getAngularVelocity ());
    }

    void handleThrow () {
        if (this.isPressed () && !this.initialized)
            initializeThrow ();
        else if (this.wasReleased ())
            throwStone ();

        if (this.initialized && this.footInStartingPosition)
            handleThrustingVelocity ();
    }

    void Update () {
        if (this.throwerController) {
            this.handleStoneObject ();
            this.handleSpawning ();
            this.handleThrow ();
        }
    }
}
