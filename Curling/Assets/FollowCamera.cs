using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	public bool follow = true;
	public Vector3 offset = new Vector3(0.0f,0.0f,0.0f);
	public Vector3 easing = new Vector3(1.0f,1.0f,1.0f);
	public bool lookAt;
	public bool startFromPosition;



	public GameObject objectToFollow;

	private Vector3 lastPos;
	private Vector3 newPos;

	// Use this for initialization
	void Start () {



		if (objectToFollow == null) {
			objectToFollow = GameObject.Find ("StoneRed");
		}

		if (lookAt || !startFromPosition) {
			lastPos = objectToFollow.transform.position + offset;
		} else {
			lastPos = gameObject.transform.position + offset;
		}
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	
		if (!follow) return;

		newPos = objectToFollow.transform.position;

		newPos = lastPos + Vector3.Scale ((newPos - lastPos), easing);

		if (lookAt) {
			gameObject.transform.LookAt( newPos );
	
		} else {
			gameObject.transform.position = newPos + offset;
		}

		lastPos = newPos;

	}

	void setFollow(bool newVal) {
		follow = newVal;
	}

}
