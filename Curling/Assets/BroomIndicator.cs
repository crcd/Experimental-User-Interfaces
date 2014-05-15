using UnityEngine;
using System.Collections;

public class BroomIndicator : MonoBehaviour {

	private GameObject arrowForward;
	private GameObject arrowLeft;
	private GameObject arrowRight;
	private GameObject arrow;
	private float blinkTime = 0;
	private bool visible;
	private float blinkSpeed = 0.4f;

	void Start () {
//		arrowForward = GameObject.Find ("ArrowForward");
//		arrowLeft = GameObject.Find ("ArrowLeft");
//		arrowRight = GameObject.Find ("ArrowRight");
		arrowForward = gameObject.transform.GetChild(0).gameObject;
		arrowLeft = gameObject.transform.GetChild(1).gameObject;
		arrowRight = gameObject.transform.GetChild(2).gameObject;


		arrowForward.renderer.enabled = false;
		arrowLeft.renderer.enabled = false;
		arrowRight.renderer.enabled = false;
		arrow = arrowForward;
		visible = true;
	}

	void Update () {

		if (arrow == null)
			Start ();
		else if (visible) {
			if ((Time.time - blinkTime) > blinkSpeed) {
				blinkTime = Time.time;
				if (arrow.renderer.enabled)
					arrow.renderer.enabled = false;
				else 
					arrow.renderer.enabled = true;
			}

		}

	}


	void ShowArrow(float speed) {
		if (speed == 0)
			visible = false;
		else {
			visible = true;
			this.blinkSpeed = (1.0f/speed);
		}
	}

	public void ShowForward (float speed) {
		ShowArrow(speed);
		arrow = arrowForward;
		arrowLeft.renderer.enabled = false;
		arrowRight.renderer.enabled = false;
	}

	public void ShowLeft (float speed) {
		ShowArrow(speed);
		arrow = arrowLeft;
		arrowForward.renderer.enabled = false;
		arrowRight.renderer.enabled = false;
	}

	public void ShowRight (float speed) {
		ShowArrow(speed);
		arrow = arrowRight;
		arrowForward.renderer.enabled = false;
		arrowLeft.renderer.enabled = false;
	}

}
