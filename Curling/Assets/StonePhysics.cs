using UnityEngine;
using System.Collections;

public class StonePhysics : MonoBehaviour {

	public float friction;
	public float broomingFactor;
	public bool brooming;
	private float tmpfriction;
	// Use this for initialization
	void Start () {
	
	}


	public float curve_amount;
	public void FixedUpdate() {
		rigidbody.AddForce(
			new Vector3(rigidbody.angularVelocity.y*curve_amount,0,0)
			);
		if (brooming) {
			tmpfriction = friction/broomingFactor;
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
