﻿using UnityEngine;
using System.Collections;

public class PsMoveThrow : Throw {
    private RUISPSMoveWand moveWand;
    public float rotationFactor;

    void Start () {
        base.Start ();
        this.moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
    }

    protected override bool wasReleased () {
        return this.moveWand.triggerButtonWasReleased;
    }

    protected override bool isPressed () {
        return this.moveWand.triggerValue > 0.9f;
    }

    protected override bool spawnButtonPressed () {
        return this.moveWand.triangleButtonWasPressed;
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
}
