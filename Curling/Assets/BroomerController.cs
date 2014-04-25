using UnityEngine;
using System.Collections;

public class BroomerController : MonoBehaviour {
	private Rigidbody stoneBody;
	private Rigidbody broomerBody;
	public Vector3 stoneOffset = new Vector3(-1,0,3);
	private Vector3 broomerStartingPos;
	private bool broomerSliding;
	private IKCtrl ikCtrl;
	
	
	void Start () {
		this.broomerBody = rigidbody;
		this.broomerStartingPos = rigidbody.position;
		this.ikCtrl = GameObject.Find ("baseMaleBroomer").GetComponent<IKCtrl> ();
	}
	
	public void setStone (Rigidbody stoneBody) {
		this.stoneBody = stoneBody;
	}
	
	private void followStone () {
		
		Vector3 newPosition = new Vector3 (
			this.stoneBody.position.x + this.stoneOffset.x,
			this.broomerBody.position.y,
			this.stoneBody.position.z + this.stoneOffset.z
			);

		this.broomerBody.position = newPosition;
		this.broomerBody.velocity = new Vector3 (0, 0, 0);
	}
	
	public void resetToStartPosition () {
		this.broomerBody.position = this.broomerStartingPos;
		this.broomerBody.velocity = new Vector3 (0, 0, 0);
		this.broomerBody.angularVelocity = new Vector3 (0, 0, 0);
	}
	
	void Update () {
		if (this.stoneBody) {
			this.followStone ();
		}
	}
}
