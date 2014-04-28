using UnityEngine;
using System.Collections;

public class UpdatePowerBar : MonoBehaviour {
	float currentPos;
	float startPos;
	float endPos;
	// Use this for initialization
	void Start () {
		this.startPos = 0.75f;
		this.currentPos = this.startPos;
		this.endPos = this.startPos + 0.19f;
	}
	
	// Update is called once per frame
	void Update () {
		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.UpArrow)) {
			this.currentPos += 0.01f;
			SetCurrentPos(this.currentPos);
		}

		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.DownArrow)) {
			this.currentPos -= 0.01f;
			SetCurrentPos(this.currentPos);
		}

	}
	
	void SetCurrentPos(float pos){
		this.currentPos = pos;
		UpdateArrowPosition ();
	}

	void UpdateArrowPosition(){
		if(this.currentPos < this.startPos ){
			this.currentPos = endPos;
		}
		if(this.currentPos > this.endPos){
			this.currentPos = this.startPos;
		}
		Debug.Log (this.startPos);
		Debug.Log (this.endPos);
		Debug.Log (this.currentPos);
		Vector3 pos = GameObject.Find ("Arrow").transform.position;
		pos.y = this.currentPos;
		GameObject.Find ("Arrow").transform.position = pos;
	}
}
