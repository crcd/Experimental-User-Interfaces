using UnityEngine;
using System.Collections;

public class PsMoveThrow : MonoBehaviour {
    public Vector3 slidingVelocityFactor;
    public Vector3 releaseVelocityFactor;
    public float rotationFactor;
    private RUISPSMoveWand moveWand;
    private ThrowerController throwerController;
    private StoneSpawner stoneSpawner;
    private bool footInStartingPosition;
    private bool throwInProgress;
    private float throwStartTime;
    private Vector3 startingMovePosition;
    public float initialDistanceToMove;
    public float distanceToMove;
    private bool initialized;

    void Start () {
        this.moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
        this.stoneSpawner = GameObject.Find ("GameLogic").GetComponent<StoneSpawner> ();
    }

    bool wasPressed () {
        return this.moveWand.triggerButtonWasPressed;
    }

    bool wasReleased () {
        return this.moveWand.triggerButtonWasReleased;
    }

    bool isPressed () {
        return this.moveWand.triggerValue > 0.9f;
    }

    bool isInitialThrowLineCrossed () {
        return this.moveWand.localPosition.z - this.startingMovePosition.z > this.initialDistanceToMove;
    }

    bool isEndlineCrossed() {
        return this.moveWand.localPosition.z - this.startingMovePosition.z > this.distanceToMove;
    }

    float getAngularVelocity () {
        return this.moveWand.angularVelocity.y * this.rotationFactor;
    }

    Vector3 getSlidingVelocity () {
        Vector3 vel = new Vector3 (this.moveWand.velocity.x, 0, this.moveWand.velocity.z);
        if (vel.z < 0) {
            vel = new Vector3 (vel.x, 0, 0);
        }
        return Vector3.Scale (vel, this.slidingVelocityFactor);
    }

    Vector3 getReleaseVelocity () {
        return Vector3.Scale (
            new Vector3 (this.moveWand.velocity.x, 0, this.moveWand.velocity.z),
            this.releaseVelocityFactor
        );
    }

    float getRotation () {
        return this.moveWand.transform.eulerAngles.y;
    }

    void handleStoneObject () {
        this.throwerController.setStoneRotation (this.getRotation ());
        if (!this.throwerController.isSawingPositionSet || this.moveWand.squareButtonWasPressed) {
            this.throwerController.isSawingPositionSet = true;
            this.throwerController.sawingStartPosition = this.moveWand.localPosition;
        } else {
            this.throwerController.sawStone (this.moveWand.localPosition);
        }
    }

    void handleSpawning () {
        if (this.moveWand.triangleButtonWasPressed) {
            this.stoneSpawner.spawnNewStone ();
            this.footInStartingPosition = true;
            this.initialized = false;
        }
    }

    void handleThrow () {
        if (this.isPressed () && !this.initialized) {
            this.footInStartingPosition = true;
            this.startingMovePosition = this.moveWand.localPosition;
            this.throwInProgress = false;
            this.initialized = true;
        } else if (this.wasReleased ()) {
            //TODO Handle if foot still in startingpos
            this.throwerController.throwStone (this.getReleaseVelocity (), this.getAngularVelocity ());
        }

        if (this.footInStartingPosition) {
            if (!this.throwInProgress) {
                if (this.isInitialThrowLineCrossed ()) {
                    this.startingMovePosition = this.moveWand.localPosition;
                    this.throwStartTime = Time.time;
                    this.throwInProgress = true;
                } 
            } else if (this.isEndlineCrossed()) {
                this.footInStartingPosition = false;
                this.throwInProgress = false;
                float timeDelta = Time.time - this.throwStartTime;
                float xDelta = this.moveWand.localPosition.x - this.startingMovePosition.x;
                Vector3 vel = new Vector3 (xDelta / timeDelta, 0, distanceToMove / timeDelta);
                this.throwerController.startSliding (Vector3.Scale (vel, this.slidingVelocityFactor));
            }
        } 
    }

    void Update () {
        if (this.throwerController) {
            this.handleStoneObject ();
            this.handleSpawning ();
            this.handleThrow ();
        }
    }
}
