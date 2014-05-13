using UnityEngine;
using System.Collections;

public class BroomController : MonoBehaviour {
	
	//	private Rigidbody broomerBody;
	private Vector3 startPosition;
	private Vector3 bodyOffset;
	public bool brooming;
	//	public float speed = 10.0f;
	//	public float amplitude = 0.1f;
	
	private Vector3 targetVelocity;
	private Vector3 velocity;
	private Vector3 velocityChange;
	
	private float[] leftBroomArea;
	private float[] centerBroomArea;
	private float[] rightBroomArea;
	private int history = 50;
	
	private float prevX = 0;
	private float currX = 0;
	private float distX = 0;
	
	private int dirX = 1;
	public float offsetCenterX;
	public float offsetAmplitude;
	public float rotationAmount;
	public float moveToBroomFactor;
	private float minX;
	private float maxX;
	private float tempX;
	
	public float distThreshold;
	
	private float centerFriction = 0.0001f; // prevent division by zero
	
	private float broomingCoefficient = 1.0f/30.0f;
	
	// Use this for initialization
	void Start () {
		
		//broomerBody = GameObject.Find ("Broomer").GetComponent<Rigidbody> ();
		//gameObject.transform.position = broomerBody.position + bodyOffset;
		
		startPosition = gameObject.transform.localPosition;
		bodyOffset = new Vector3 (0,0,0);
		
		leftBroomArea = new float[history];
		rightBroomArea = new float[history];
		centerBroomArea = new float[history];
		
	}
	
	public void updateBroom(float prevX, float currX, float distX) {
		this.prevX = prevX;
		this.currX = currX;
		this.distX = distX;	
	}
	
	public float getCenterFriction() {
		return this.centerFriction;
	}
	
	void FixedUpdate () {
		
		
		
		Vector3 offset = bodyOffset;
		
		//		if (brooming) {
		//			offset.z += Mathf.Abs(Mathf.Sin(Time.time * speed)) * amplitude ;
		//		}
		
		if (currX > prevX) {
			dirX = 1;
		} else {
			dirX = -1;
		}
		
		//Debug.Log (distX);
		
//		if (distX > distThreshold) distX = distThreshold;
//		
//		offset.z += dirX * distX * moveToBroomFactor;


		//Debug.Log ("Current x pos:" + currX);

		// Broom should be adjusted 
		
		minX = offsetCenterX - offsetAmplitude;
		maxX = offsetCenterX + offsetAmplitude;
//		
//		if (offset.z > maxX) offset.z = maxX;
//		if (offset.z < minX) offset.z = minX;

		tempX = currX;

		if (tempX < minX) tempX = minX;
		if (tempX > maxX) tempX = maxX;

		offset.z = (tempX - offsetCenterX) * moveToBroomFactor;


		
		bodyOffset = offset;
		
		float rotX = ((tempX - offsetCenterX) / offsetAmplitude) * -rotationAmount;
		
		//gameObject.transform.position = broomerBody.position + offset;
		
		Debug.Log (offset);
		
		gameObject.transform.localPosition = startPosition + offset;
		
		//		gameObject.rigidbody.MovePosition (new Vector3 (
		//			broomerBody.position.x + offset.z,
		//            0,
		//			broomerBody.position.z + offset.z
		//		));
		
		//		gameObject.transform.position.Set (
		//			broomerBody.position.x + offset.z,
		//			0,
		//			broomerBody.position.z + offset.z
		//		);
		
		//Debug.Log (rotX);
		
		gameObject.transform.eulerAngles = new Vector3 (rotX, 90, 0);
		
		//Debug.Log ("PrevX: " + prevX + " CurrX: " + currX + " DistX: " + distX);
		
		var tempBroom = new float[history];
		tempBroom [0] = distX;
		for ( int i = 1; i < history; i++ ) {
			tempBroom[i] = centerBroomArea[i-1];
		}
		centerBroomArea = tempBroom;
		
		float sumArea = 0.0001f; // prevent zero division
		for ( int i = 0; i < history; i++ ) {
			sumArea = sumArea + tempBroom[i];
		}
		if (sumArea > 30) {
			sumArea = 30;
		}
		// sumArea varies now between 0 and 30
		centerFriction = sumArea * broomingCoefficient;
		
		
		
		//Debug.Log ("sum of area: " + sumArea);
	}
}
