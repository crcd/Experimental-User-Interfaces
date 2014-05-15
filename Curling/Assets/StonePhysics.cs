using UnityEngine;
using System.Collections;

public class StonePhysics : MonoBehaviour {

	public float friction;
	public float broomingFactor;
	//public bool brooming;
	private BroomController broomController;

	public float angularVelocityFactor = 0.05f;
	public float angularVelocityThreshold = 0.01f;

	private GameObject movingStone;
	private StoneFinder stoneFinder;

	// Use this for initialization
	void Start () {
		broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
		if (stoneFinder == null) stoneFinder = gameObject.AddComponent<StoneFinder> ();
	}
	
	public float curveAmount;
	public void FixedUpdate() {
		this.broomingFactor = broomController.getCenterFriction ();
        //Debug.Log ("Brooming factor: " + this.broomingFactor);

		movingStone = stoneFinder.findMovingStone ();

//		if (movingStone != null) Debug.Log (rigidbody == movingStone.rigidbody);

		if (movingStone != null && rigidbody == movingStone.rigidbody) {

			float leftFriction = broomController.getLeftFriction () * angularVelocityFactor;
			float rightFriction = -broomController.getRightFriction () * angularVelocityFactor;

			Vector3 aVel = movingStone.rigidbody.angularVelocity;

			movingStone.rigidbody.angularVelocity = new Vector3 (
				aVel.x,
				aVel.y + leftFriction + rightFriction,
				aVel.z
			);

			float tmpFriction = friction;

			if (broomingFactor > 0.1f) {
				tmpFriction = friction - (0.8f * friction) * this.broomingFactor; // Remove max 80%
			}

			Vector3 movingFrictionForce = -1 * tmpFriction * rigidbody.velocity.normalized;

			rigidbody.AddForce (movingFrictionForce);

		} else {
			Vector3 frictionForce = -1 * friction * rigidbody.velocity.normalized;
			rigidbody.AddForce (frictionForce);
		}

		// Kill angular velocity at low speed
		if (rigidbody.velocity.magnitude < angularVelocityThreshold) {
			rigidbody.angularVelocity = new Vector3 (
				0,
				rigidbody.angularVelocity.y * 0.5f,
				0
			);
		}

		rigidbody.AddForce(
			new Vector3(rigidbody.angularVelocity.y*curveAmount,0,0) *
			rigidbody.velocity.magnitude
		);

	}
}
