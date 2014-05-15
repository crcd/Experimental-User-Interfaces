using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    public Vector3 stoneOffsetConfig;
    public Vector3 minOffset;
    public Vector3 maxOffset;
    public Vector3 minSlidingSpeed;
    public Vector3 maxSlidingSpeed;
    public Vector3 maxThrowingSpeed;
    public Vector3 slidingForceFactor;
    public Vector3 maxSlidingForceAdder;
    private GameObject stone;
    private Rigidbody stoneBody;
    private Rigidbody throwerBody;
    private Vector3 stoneOffset;
    private Vector3 throwerStartingPos;
    private bool throwerSliding;
    private IKCtrl ikCtrl;
    private BroomController broomController;
    private UpdatePowerBar powerBarUpdater;

    void Start () {
        this.throwerBody = rigidbody;
        this.throwerStartingPos = rigidbody.position;
        this.ikCtrl = GameObject.Find ("baseMaleThrower").GetComponent<IKCtrl> ();
        this.broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
        this.powerBarUpdater = gameObject.AddComponent<UpdatePowerBar> ();
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
        this.powerBarUpdater.SetPowerPercentage (0);
    }

    public GameObject getStone () {
        return this.stone;
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

    public void displayPowerPercentage (float percentage) {
        this.powerBarUpdater.SetPowerPercentage (percentage);
    }

    public void startSlidingToScale (Vector3 powerScale) {
        displayPowerPercentage (powerScale.z * 100);
        Vector3 slidingSpeed = Vector3.Scale (
            this.maxSlidingSpeed - this.minSlidingSpeed,
            powerScale
        ) + this.minSlidingSpeed;
        slidingSpeed.z = getForceFromScale (powerScale.z * 100);
        this.throwerBody.AddForce (slidingSpeed, ForceMode.VelocityChange);
    }

    float getForceFromScale (float powerScale) {
        /*0: 4
        50: 4.5
        80: 5
        90: 6
        100: 7*/
        float k = 0;
        float power = 0;
        if (powerScale >= 0 && powerScale <= 70f) {
            k = (4.5f - 3.5f) / (70f - 0);
            power = 3.5f + powerScale * k;
        } else if (powerScale > 70 && powerScale <= 90f) {
            k = (6f - 4.5f) / (90f - 70f);
            power = 4.5f + (powerScale - 70f) * k;
        } else if (powerScale > 90) {
            k = (7f - 6f) / (100f - 90f);
            power = 6.0f + (powerScale - 90f) * k;
        }
        return power;

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
        Vector3 defaultOffset = this.stoneOffsetConfig + offset;
        Vector3 minOffset = Vector3.Max (defaultOffset, this.minOffset);
        Vector3 maxOffset = Vector3.Min (minOffset, this.maxOffset);
        this.stoneOffset = maxOffset;
    }

    private void keepStonePositionInHand () {
        // Set velocity if the thrower is moving
        if (this.throwerBody.velocity.z > 0.01) {
			this.stoneBody.velocity = this.throwerBody.velocity;
		} else {
			Vector3 newPosition = new Vector3 (
				this.throwerBody.position.x + stoneOffset.x,
				this.stoneBody.position.y,
				this.throwerBody.position.z + stoneOffset.z
			);
			this.stoneBody.position = newPosition;
		}
    }

    void Update () {
        if (this.stoneBody) {
            this.keepStonePositionInHand ();
        }
    }
}
