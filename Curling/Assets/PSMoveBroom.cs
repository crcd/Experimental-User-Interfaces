using UnityEngine;
using System.Collections;

public class PSMoveBroom : MonoBehaviour {

	private RUISPSMoveWand moveWand;

	private BroomController broomController;

	// Use this for initialization
	void Start () {

		moveWand = gameObject.GetComponent<RUISPSMoveWand> ();
		broomController = GameObject.Find ("Broom").GetComponent<BroomController> ();

	}
	
	// Update is called once per frame
	void Update () {
		//broomController.rotation = moveWand.localRotation;


	}
}
