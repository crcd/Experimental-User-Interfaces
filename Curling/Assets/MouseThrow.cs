using UnityEngine;
using System.Collections;

public class MouseThrow : Throw {
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
        return new Vector3 (0, 0, 0);
    }

    protected override Vector3 getControllerPosition () {
        return new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y);
    }
}
