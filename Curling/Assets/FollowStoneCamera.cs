using UnityEngine;
using System.Collections;

public class FollowStoneCamera : MonoBehaviour {

	public bool follow;
	public Vector3 offset;

	private GameObject stone;

	// Use this for initialization
	void Start () {

		follow = true;
		stone = GameObject.Find("StoneRed");
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (!follow) return;

		gameObject.transform.position = stone.transform.position + offset;

	}

	void setFollow(bool newVal) {
		follow = newVal;
	}

}
