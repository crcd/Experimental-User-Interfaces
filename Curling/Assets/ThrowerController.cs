using UnityEngine;
using System.Collections;

public class ThrowerController : MonoBehaviour {
    public Vector3 stoneOffsetConfig;
    public float maxDistanceFromBody;
	public float minOffsetZ;
	public float maxOffsetZ;
	public float minOffsetX;
	public float maxOffsetX;
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
            this.stoneBody.velocity = this.throwerBody.velocity + additionalVelocity;
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

        // Just for testing
        this.broomController.brooming = true;


    }

    public void startSliding (Vector3 force) {
        if (this.stoneBody) {
            this.stoneBody.angularVelocity = new Vector3 (0, 0, 0);
        }
        this.throwerSliding = true;
        this.throwerBody.AddForce (force, ForceMode.VelocityChange);
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
		if(offset.z > this.maxOffsetZ){
			offset.z = this.maxOffsetZ;
		}
		if(offset.z < this.minOffsetZ){
			offset.z = this.minOffsetZ;
		}
		if(offset.x > this.maxOffsetX){
			offset.x = this.maxOffsetX;
		}
		if(offset.x < this.minOffsetX){
			offset.x = this.minOffsetX;
		}

        //this.stoneOffset = Vector3.ClampMagnitude (offset, this.maxDistanceFromBody);
		this.stoneOffset = offset;
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
