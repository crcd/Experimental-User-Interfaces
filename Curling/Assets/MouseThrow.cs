using UnityEngine;
using System.Collections;

public class MouseThrow : Throw {
    private Vector3 previousMousePosition;

    protected override bool wasReleased () {
        return Input.GetMouseButtonUp (0);
    }

    protected override bool isPressed () {
        return Input.GetMouseButtonDown (0);
    }

    protected override bool spawnButtonPressed () {
        return Input.GetMouseButtonDown (1);
    }

    protected override float getAngularVelocity () {
        return 0;
    }

    protected override float getRotation () {
        return 0;
    }

    protected override Vector3 getControllerVelocity () {
        float timeFactor = 1 / Time.deltaTime;
        Vector3 timeScaled = Vector3.Scale (
                                 Input.mousePosition - this.previousMousePosition,
                                 new Vector3 (timeFactor, timeFactor, timeFactor)
                             );

        return new Vector3 (timeScaled.x, 0, timeScaled.y);
    }

    protected override Vector3 getControllerPosition () {
        return new Vector3 (Input.mousePosition.x, 0, Input.mousePosition.y);
    }

    void Update () {
        base.FixedUpdate ();
        this.previousMousePosition = Input.mousePosition;
    }
}
