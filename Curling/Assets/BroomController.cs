﻿using UnityEngine;
using System.Collections;

public class BroomController : MonoBehaviour {

	private Rigidbody broomerBody;
	public Vector3 bodyOffset;
	public bool brooming;
	public float speed = 10.0f;
	public float amplitude = 0.1f;

	private Vector3 targetVelocity;
	private Vector3 velocity;
	private Vector3 velocityChange;

	// Use this for initialization
	void Start () {

		broomerBody = GameObject.Find ("Broomer").GetComponent<Rigidbody> ();
		gameObject.transform.position = broomerBody.position + bodyOffset;
	
	}


	void FixedUpdate () {
	
		Vector3 offset = bodyOffset;

		if (brooming) {
			offset.x += Mathf.Abs(Mathf.Sin(Time.time * speed)) * amplitude ;
		}

		//gameObject.transform.position = broomerBody.position + offset;

		gameObject.transform.position = new Vector3 (
			broomerBody.position.x + offset.x,
            0,
			broomerBody.position.z + offset.z
		);


	}
}
