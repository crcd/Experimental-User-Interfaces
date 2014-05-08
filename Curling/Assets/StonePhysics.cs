using UnityEngine;
using System.Collections;

public class StonePhysics : MonoBehaviour {

	public float friction;
	public float broomingEffect;
	// Use this for initialization
	void Start () {
	
	}


	public float curve_amount;
	public void FixedUpdate() {
		rigidbody.AddForce(
			new Vector3(rigidbody.angularVelocity.y*curve_amount,0,0)
			);
		Vector3 frictionForce = -1*friction*rigidbody.velocity.normalized;
		rigidbody.AddForce(
				frictionForce
			);
	}
}
