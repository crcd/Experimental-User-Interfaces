using UnityEngine;
using System.Collections;

public class PsMoveThrow : Throw {
    private RUISPSMoveWand moveWand;
    public float rotationFactor;

    new void Start () {
        base.Start ();
        this.moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
    }

    protected override bool wasReleased () {
        return this.moveWand.triggerButtonWasReleased;
    }

    public override bool isPressed () {
        return this.moveWand.triggerValue > 0.9f;
    }

    protected override bool leftMovePressed () {
        return this.moveWand.squareButtonDown;
    }

    protected override bool rightMovePressed () {
        return this.moveWand.triangleButtonDown;
    }

    protected override bool spawnButtonPressed () {
        return this.moveWand.moveButtonDown;
    }

    protected override float getAngularVelocity () {
        return this.moveWand.angularVelocity.y * this.rotationFactor;
    }

    protected override float getRotation () {
        return this.moveWand.transform.eulerAngles.y;
    }

    protected override Vector3 getControllerVelocity () {
        return this.moveWand.velocity;
    }

    protected override Vector3 getControllerPosition () {
        return this.moveWand.localPosition;
    }

    protected override bool resetRoundButtonPressed () {
        return this.moveWand.startButtonWasPressed;
    }

    protected override void activateRumble () {
        this.moveWand.RumbleOn (15);
    }

    protected override void disableRumble () {
        this.moveWand.RumbleOff ();
    }
}
