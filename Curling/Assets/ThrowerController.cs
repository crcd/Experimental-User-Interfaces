using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    public Vector3 stoneOffset;
    private bool keepStone;

    void Start () {
        this.stoneBody = GameObject.Find ("StoneRed").rigidbody;
        this.throwerBody = rigidbody;
        keepStone = true;
	}

    public void holdStone() {
        Vector3 newPosition = new Vector3 (
                                  this.throwerBody.position.x + stoneOffset.x,
                                  this.stoneBody.position.y,
                                  this.throwerBody.position.z + stoneOffset.z
                              );
        this.stoneBody.position = newPosition;
    }

    public void throwStone (Vector3 additionalVelocity, float yRotation) {
        if (this.stoneBody != null) {
            this.stoneBody.angularVelocity = new Vector3 (0, yRotation, 0);
            this.stoneBody.velocity = this.throwerBody.velocity + additionalVelocity;
            this.keepStone = false;
        }
    }

    public void startSliding (Vector3 velocity) {
        if (this.stoneBody != null) {
            this.stoneBody.angularVelocity = new Vector3 (0, 0, 0);
        }
        this.throwerBody.velocity = velocity;
    }

	void Update () {
        if (this.keepStone) {
            this.holdStone ();
        }
	}
}
