using UnityEngine;
using System.Collections;

public class LaunchStone : MonoBehaviour {

	private RUISPSMoveWand moveWand;
	private FollowStoneCamera camera;

	private Rigidbody stoneBody;
	public float velocity;
	public float angularVelocity;

	// Use this for initialization
	void Start () {

		moveWand = gameObject.GetComponent<RUISPSMoveWand> ();

		camera = GameObject.Find("RUISCamera").GetComponent<FollowStoneCamera>();

		stoneBody = GameObject.Find("StoneRed").rigidbody;

	}
	
	// Update is called once per frame
	void Update () {

		if (!stoneBody) return;

		if (moveWand.triggerButtonWasPressed) {

			stoneBody.velocity = new Vector3(0,0,velocity);
			// Debug.Log("Trigger button was pressed!!!");
		}

		if (moveWand.triggerButtonWasReleased) {
			//Debug.Log("Trigger button was released!!!");

			camera.follow = false;
			stoneBody.angularVelocity = new Vector3(0,angularVelocity,0);

		}


	
	}
}
