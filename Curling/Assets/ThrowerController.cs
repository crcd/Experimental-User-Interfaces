using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    public Vector3 stoneOffsetConfig;
    public Vector3 minOffset;
    public Vector3 maxOffset;
    public Vector3 minSlidingSpeed;
    public Vector3 maxSlidingSpeed;
    public Vector3 maxThrowingSpeed;
    private GameObject stone;
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    private Vector3 stoneOffset;
    private Vector3 throwerStartingPos;
    private bool throwerSliding;
    private IKCtrl ikCtrl;
    private BroomController broomController;

    void Start () {
        this.throwerBody = rigidbody;
        this.throwerStartingPos = rigidbody.position;
        this.ikCtrl = GameObject.Find ("baseMaleThrower").GetComponent<IKCtrl> ();
        this.broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
    }

    public void setStone (GameObject stone) {
        this.stone = stone;

        this.stoneBody = stone.rigidbody;
        this.stoneBody.position = new Vector3 (
            this.stoneOffsetConfig.x,
            this.stoneBody.position.y,
            this.stoneOffsetConfig.z
        );
        this.ikCtrl.rightHandObj = stoneBody.transform;
        this.ikCtrl.rightIKActive = true;
    }

    public void throwStone (Vector3 additionalVelocity, float yRotation) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, yRotation, 0);
            this.stoneBody.velocity = this.throwerBody.velocity + this.getLimitedThrowingForce (additionalVelocity);
            if (this.stone.tag == "CurrentYellowStone")
                this.stone.tag = "MovingYellowStone";
            else
                this.stone.tag = "MovingRedStone";
            this.stone = null;
            this.stoneBody = null;
        }
        if (throwerSliding)
            throwerSliding = false;
        // this.ikCtrl.rightHandObj = null; // IKCtrl has animation
        this.ikCtrl.rightIKActive = false;


    }

    public void startAddingSlidingForce () {
        this.throwerSliding = true;
    }

    public void startSliding () {
        Debug.Log (this.throwerBody.velocity);
        accelerateToMinimumSlidingVelocity ();
    }

    public void accelerateToMinimumSlidingVelocity () {
        Vector3 optionalVelocity = Vector3.Max (this.throwerBody.velocity, this.minSlidingSpeed);
        if (this.throwerBody.velocity.z < this.minSlidingSpeed.z) {

            Vector3 velocityChange = new Vector3 (0, 0, this.minSlidingSpeed.z - this.throwerBody.velocity.z);
            this.throwerBody.AddForce (velocityChange, ForceMode.VelocityChange);
        }
    }

    public void addSlidingForce (Vector3 force) {
        Vector3 slidingForce = Vector3.Scale (force, new Vector3 (1f, 0, 1f));
        if (this.throwerBody.velocity.z < this.maxSlidingSpeed.z) {

            this.throwerBody.AddForce (new Vector3 (0, 0, slidingForce.z), ForceMode.Impulse);
        }
        if (this.throwerBody.velocity.x < this.maxSlidingSpeed.x) {
            this.throwerBody.AddForce (new Vector3 (slidingForce.x, 0, 0), ForceMode.Impulse);
        }
        //this.throwerBody.AddForce (slidingForce, ForceMode.Impulse);
    }

    Vector3 getLimitedSlidingForce (Vector3 force) {
        Vector3 minForce = Vector3.Max (force, this.minSlidingSpeed);
        Vector3 maxForce = Vector3.Min (minForce, this.maxSlidingSpeed);
        return maxForce;
    }

    Vector3 getLimitedThrowingForce (Vector3 force) {
        Vector3 minForce = Vector3.Max (force, Vector3.zero);
        Vector3 maxForce = Vector3.Min (minForce, this.maxThrowingSpeed);
        return maxForce;
    }

    public void resetToStartPosition () {
        this.throwerBody.position = this.throwerStartingPos;
        this.throwerBody.velocity = new Vector3 (0, 0, 0);
        this.throwerBody.angularVelocity = new Vector3 (0, 0, 0);

        // Just for testing
        this.broomController.brooming = false;
		
    }

    public void setStoneRotation (float stoneRotation) {
        if (this.stoneBody) {
            this.stoneBody.transform.eulerAngles = new Vector3 (0, stoneRotation, 0);
        }
    }

    public void sawStone (Vector3 offset) {
        Vector3 minOffset = Vector3.Max (offset, this.minOffset);
        Vector3 maxOffset = Vector3.Min (minOffset, this.maxOffset);
        this.stoneOffset = maxOffset;
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
        this.stoneBody.position = newPosition;
    }

    void Update () {
        if (this.stoneBody) {
            this.keepStonePositionInHand ();
        }
    }
}
