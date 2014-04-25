using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    public Vector3 stoneOffsetConfig;
    public Vector3 sawingStartPosition;
    public bool isSawingPositionSet = false;
    private Vector3 stoneOffset;
	private Vector3 throwerStartingPos;
	private bool throwerSliding;
	private Vector3 throwerSlidingForce;
    private IKCtrl ikCtrl;


    void Start () {
        this.throwerBody = rigidbody;
		this.throwerStartingPos = rigidbody.position;
        this.ikCtrl = GameObject.Find ("baseMaleThrower").GetComponent<IKCtrl> ();
    }

    public void setStone (Rigidbody stoneBody) {
        this.stoneBody = stoneBody;
        this.stoneOffset = this.stoneOffsetConfig;
		this.ikCtrl.rightHandObj = stoneBody.transform;
        this.ikCtrl.rightIKActive = true;
    }

    public void throwStone (Vector3 additionalVelocity, float yRotation) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, yRotation, 0);
            this.stoneBody.velocity = this.throwerBody.velocity + additionalVelocity;
            this.stoneBody = null;
        }
		if (throwerSliding) throwerSliding = false;
        // this.ikCtrl.rightHandObj = null; // IKCtrl has animation
		this.ikCtrl.rightIKActive = false;
    }

    public void startSliding (Vector3 force) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, 0, 0);
        }
        // this.throwerBody.velocity = velocity;
		this.throwerSliding = true;
		this.throwerSlidingForce = force;
        //this.throwerBody.AddForce(this.throwerSlidingForce, ForceMode.Impulse);

    }

    public void resetToStartPosition () {
		this.throwerBody.position = this.throwerStartingPos;
        this.throwerBody.velocity = new Vector3 (0, 0, 0);
        this.throwerBody.angularVelocity = new Vector3 (0, 0, 0);
    }

    public void setStoneRotation (float stoneRotation) {
        if (this.stoneBody) {
            this.stoneBody.transform.eulerAngles = new Vector3 (0, stoneRotation, 0);
        }
    }

    public void sawStone (Vector3 controllerPosition) {
        if (this.stoneBody != null && this.sawingStartPosition != null) {
            Vector3 newPosition = new Vector3 (
                                      this.sawingStartPosition.x - controllerPosition.x,
                                      0,
                                      this.sawingStartPosition.z - controllerPosition.z
                                  );
            this.stoneOffset = this.stoneOffsetConfig - newPosition;
        }
    }

    private void keepStonePositionInHand () {
        //A bit of a workaround to give stone velocity for the sound
        if (this.throwerBody.velocity.z > 0.01) {
            this.stoneBody.velocity = this.throwerBody.velocity;
        }
        Vector3 newPosition = new Vector3 (
                                  this.throwerBody.position.x + stoneOffset.x,
                                  this.stoneBody.position.y,
                                  this.throwerBody.position.z + stoneOffset.z
                              );
        this.stoneBody.MovePosition (newPosition);
        //this.stoneBody.position = newPosition;
    }

    void Update () {
        if (this.stoneBody) {
            this.keepStonePositionInHand ();
        }

        if (this.throwerBody && this.throwerSliding) {
        	this.throwerBody.AddForce(this.throwerSlidingForce, ForceMode.Impulse);
        }
    }
}
