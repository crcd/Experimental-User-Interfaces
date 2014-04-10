using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    public Vector3 stoneOffset;

    void Start () {
        this.throwerBody = rigidbody;
    }

    public void setStone (Rigidbody stoneBody) {
        this.stoneBody = stoneBody;
    }

    public void throwStone (Vector3 additionalVelocity, float yRotation) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, yRotation, 0);
            this.stoneBody.velocity = this.throwerBody.velocity + additionalVelocity;
            this.stoneBody = null;
        }
    }

    public void startSliding (Vector3 velocity) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, 0, 0);
        }
        this.throwerBody.velocity = velocity;
    }

    public void resetToStartPosition () {
        this.throwerBody.position = new Vector3 (0, 0, -20.5f);
        this.throwerBody.velocity = new Vector3 (0, 0, 0);
        this.throwerBody.angularVelocity = new Vector3 (0, 0, 0);
    }

    public void setStoneRotation (float stoneRotation) {
        if (this.stoneBody) {
            this.stoneBody.rotation = new Quaternion (0, stoneRotation, 0, 0);
        }
    }

    private void keepStonePositionInHand () {
        Vector3 newPosition = new Vector3 (
                                  this.throwerBody.position.x + stoneOffset.x,
                                  this.stoneBody.position.y,
                                  this.throwerBody.position.z + stoneOffset.z
                              );
        this.stoneBody.position = newPosition;
    }

    void Update () {
        if (this.stoneBody) {
            this.keepStonePositionInHand ();
        }
    }
}
