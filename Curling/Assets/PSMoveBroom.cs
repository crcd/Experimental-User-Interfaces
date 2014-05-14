using UnityEngine;
using System.Collections;

public class PSMoveBroom : MonoBehaviour {

	private RUISPSMoveWand moveWand;

	private BroomController broomController;

	private float prevX = 0;
	private float currX = 0;
	private float distX = 0;
	private float currZ = 0;

	// Use this for initialization
	void Start () {

		moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
		broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();
		prevX = moveWand.localPosition.x * 10;
		currX = moveWand.localPosition.x * 10;
	}
	
	// Update is called once per frame
	void Update () {
		//broomController.rotation = moveWand.localRotation;
	}

	void FixedUpdate () {
		prevX = currX;
		currX = moveWand.localPosition.x * 10;
		distX = Mathf.Abs (currX - prevX);

		currZ = moveWand.localPosition.z * 10;

		broomController.updateBroom (prevX, currX, distX, currZ);
	}
}
