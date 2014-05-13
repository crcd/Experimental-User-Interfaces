using UnityEngine;
using System.Collections;

public abstract class Throw : MonoBehaviour {
    public Vector3 releaseVelocityFactor;
    public Vector3 handlingFactor;
    public Vector3 minDistance;
    public Vector3 maxDistance;
    public float minControllerThrowVelocity;
    protected ThrowerController throwerController;
    protected GameLogic gameLogic;
    protected Vector3 startingMovePosition;
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

    abstract protected bool resetRoundButtonPressed ();

    abstract protected float getAngularVelocity ();

    abstract protected float getRotation ();

    abstract protected Vector3 getControllerVelocity ();

    abstract protected Vector3 getControllerPosition ();

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
        this.minVelocityReached = false;
    }

    void releaseFoot () {
        this.footInStartingPosition = false;
        this.minVelocityReached = false;
        this.throwerController.startSlidingToScale (getDistanceScale ());
    }

    bool isControllerOverMinVelocity () {
        return getControllerVelocity ().z > this.minControllerThrowVelocity;
    }

    Vector3 getTravelledDistance () {
        return getControllerPosition () - startingMovePosition;
    }

    Vector3 getDistanceScale () {
        Vector3 clampedDistance = Vector3.Min (getTravelledDistance (), this.maxDistance) - this.minDistance;
        Vector3 distanceLimits = this.maxDistance - this.minDistance;
        return new Vector3 (
            clampedDistance.x / distanceLimits.x,
            0,
            clampedDistance.z / distanceLimits.z
        );
    }

    void handleThrustingVelocity () {
        if (!isPressed ())
            return;
        if (!this.minVelocityReached) {
            if (isControllerOverMinVelocity ()) {
                startingMovePosition = this.getControllerPosition ();
                minVelocityReached = true;
            }
                
        } else {
            if (isControllerOverMinVelocity ()) {
                this.throwerController.displayPowerPercentage (getDistanceScale ().z * 100f);
            } else
                releaseFoot ();
        }
    }

    void throwStone () {
        this.throwerController.throwStone (this.getReleaseVelocity (), this.getAngularVelocity ());
        this.initialized = false;
    }

    void handleThrow () {
        if (this.isPressed () && !this.initialized)
            initializeThrow ();
        if (initialized) {
            if (footInStartingPosition)
                handleThrustingVelocity ();
            else if (isStoneOverThrowLine() || this.wasReleased () || !this.isPressed ())
                throwStone ();
        }
    }

    bool isStoneOverThrowLine () {
        if (throwerController.getStone ())
            return throwerController.getStone ().transform.position.z > -11f;
        return false;
    }

    protected void FixedUpdate () {
        if (this.resetRoundButtonPressed ())
            this.gameLogic.resetRound ();
        if (this.throwerController) {
            this.handleStoneObject ();
            this.handleSpawning ();
            this.handleThrow ();
        }
    }
}
