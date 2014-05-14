using UnityEngine;
using System.Collections;

public class DeleteRotation : MonoBehaviour {
	
	private GameObject rotater;
	private float yrot;
	
	void Start () {
		rotater = GameObject.Find ("RotationHelper");
	}

	void FixedUpdate () {
		if (rotater != null) {
			yrot = rotater.transform.parent.rotation.y;
			rotater.transform.rotation = Quaternion.Euler(0, -1.0f * yrot, 0);
		}
	}
}
