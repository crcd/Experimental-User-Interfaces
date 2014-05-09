using UnityEngine;
using System.Collections;

public class StonePhysics : MonoBehaviour {

	public float friction;
	public float broomingFactor;
	//public bool brooming;
	private float tmpfriction;
	private BroomController broomController;
	// Use this for initialization
	void Start () {
		broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
	}
	
	public float curve_amount;
	public void FixedUpdate() {
		this.broomingFactor = broomController.getCenterFriction ();
		//Debug.Log ("brooming factor" + this.broomingFactor);
		rigidbody.AddForce(
			new Vector3(rigidbody.angularVelocity.y*curve_amount,0,0)
			);
		if (broomingFactor > 0.1) {
			tmpfriction = friction/this.broomingFactor;
		}
		else {
			tmpfriction = friction;
		}
		Vector3 frictionForce = -1*tmpfriction*rigidbody.velocity.normalized;
		rigidbody.AddForce(
				frictionForce
			);
	}
}
