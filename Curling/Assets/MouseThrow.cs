﻿using UnityEngine;
using System.Collections;

public class MouseThrow : Throw {
    private Vector3 previousMousePosition;

    protected override bool wasReleased () {
        return Input.GetMouseButtonUp (0);
    }

    public override bool isPressed () {
        return Input.GetMouseButton (0);
    }

    protected override bool leftMovePressed () {
        return Input.GetKeyDown("k");
    }

    protected override bool rightMovePressed () {
        return Input.GetKeyDown("l");
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

    protected override bool resetRoundButtonPressed () {
        return Input.GetKeyUp ("r");
    }

    protected override void activateRumble () {
    }

    protected override void disableRumble () {
    }

    void Update () {
        base.Update ();
        this.previousMousePosition = Input.mousePosition;
    }
}
