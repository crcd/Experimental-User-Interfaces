using UnityEngine;
using System.Collections;

public class PsMoveThrow : MonoBehaviour {
    private RUISPSMoveWand moveWand;
    private ThrowerController throwerController;
    private bool throwInProgress;
    private Vector3 maxAcceleration;
    public Vector3 slidingVelocityFactor;
    public Vector3 releaseVelocityFactor;
    public float rotationFactor;
    // Use this for initialization
    void Start () {
        this.moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
        Debug.Log (this.moveWand);
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
    }

    bool wasPressed () {
        return this.moveWand.triggerButtonWasPressed;
    }

    bool wasReleased () {
        return this.moveWand.triggerButtonWasReleased;
    }

    float getAngularVelocity() {
        return this.moveWand.angularVelocity.y * this.rotationFactor;
    }

    Vector3 getSlidingVelocity() {

        return Vector3.Scale(
            new Vector3(this.moveWand.velocity.x, 0, this.moveWand.velocity.z),
            this.slidingVelocityFactor
        );
    }

    Vector3 getReleaseVelocity() {
        return Vector3.Scale(
            new Vector3(this.moveWand.velocity.x, 0, this.moveWand.velocity.z),
            this.releaseVelocityFactor
        );
    }

    float getRotation() {
        return this.moveWand.localRotation.y;
    }

    void Update () {
        if (this.throwerController) {
            this.throwerController.setStoneRotation (this.getRotation ());
            if (this.wasPressed ()) {
                this.throwerController.startSliding (this.getSlidingVelocity ());
                this.throwInProgress = true;
            } else if (this.wasReleased ()) {
                this.throwInProgress = false;
                this.throwerController.throwStone (this.getReleaseVelocity (), this.getAngularVelocity ());
            } else if (this.throwInProgress) {
                this.maxAcceleration = this.moveWand.acceleration;
            }
        }
    }
}
