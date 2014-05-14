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

	private float[] tempLeftBroom;
	private float[] tempCenterBroom;
	private float[] tempRightBroom;

	private int history = 50;
	
	private float prevX = 0;
	private float currX = 0;
	private float distX = 0;
	private float currZ = 0;
	
	private int dirX = 1;
	public float offsetCenterX;
	public float offsetCenterZ;
	public float offsetAmplitudeX; //2.5
	public float offsetAmplitudeZ;
	public float rotationAmountX;
	public float rotationAmountZ;
	public float moveToBroomFactor;
	private float minX;
	private float maxX;
	private float minZ;
	private float maxZ;
	private float tempZ;
	private float tempX;
	
	public float distThreshold;
	
	private float leftFriction = 0.0001f; // prevent division by zero
	private float centerFriction = 0.0001f; // prevent division by zero
	private float rightFriction = 0.0001f; // prevent division by zero
	
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
		tempLeftBroom = new float[history];
		tempRightBroom = new float[history];
		tempCenterBroom = new float[history];
		
	}
	
	public void updateBroom(float prevX, float currX, float distX, float currZ) {
		this.prevX = prevX;
		this.currX = currX;
		this.distX = distX;
		this.currZ = currZ;
	}

	public float getLeftFriction() {
		return this.leftFriction;
	}

	public float getCenterFriction() {
		return this.centerFriction;
	}

	public float getRightFriction() {
		return this.rightFriction;
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
		
		minX = offsetCenterX - offsetAmplitudeX;
		maxX = offsetCenterX + offsetAmplitudeX;

		minZ = offsetCenterZ - offsetAmplitudeZ;
		maxZ = offsetCenterZ + offsetAmplitudeZ;

//		
//		if (offset.z > maxX) offset.z = maxX;
//		if (offset.z < minX) offset.z = minX;

		tempX = currX;
		tempZ = currZ;

		if (tempX < minX) tempX = minX;
		if (tempX > maxX) tempX = maxX;
		if (tempZ < minZ) tempZ = minZ;
		if (tempZ > maxZ) tempZ = maxZ;

		offset.z = -(tempX - offsetCenterX) * moveToBroomFactor;

		offset.x = (tempZ - offsetCenterZ) * moveToBroomFactor;


		
		bodyOffset = offset;
		
		float rotX = ((tempX - offsetCenterX) / offsetAmplitudeX) * rotationAmountX;
		float rotZ = ((tempZ - offsetCenterZ) / offsetAmplitudeZ) * rotationAmountZ;
		
		//gameObject.transform.position = broomerBody.position + offset;
		
		//Debug.Log (offset);
		
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
		
		gameObject.transform.eulerAngles = new Vector3 (rotX, -90, rotZ);
		
		//Debug.Log ("PrevX: " + prevX + " CurrX: " + currX + " DistX: " + distX);




		if (offset.z >= -0.5 && offset.z < 0.3*(-0.5)) {
			// Left area
			tempLeftBroom[0] = distX;
			tempCenterBroom[0] = 0.0f;
			tempRightBroom[0] = 0.0f;
		} else if (offset.z >= 0.3*(-0.5) && offset.z < 0.3*0.5) {
			// Center area
			tempLeftBroom[0] = 0.0f;
			tempCenterBroom[0] = distX;
			tempRightBroom[0] = 0.0f;
		} else {
			// Right area
			tempLeftBroom[0] = 0.0f;
			tempCenterBroom[0] = 0.0f;
			tempRightBroom[0] = distX;
		}

		for ( int i = 1; i < history; i++ ) {
			tempLeftBroom[i] = tempLeftBroom[i-1];
			tempCenterBroom[i] = tempCenterBroom[i-1];
			tempRightBroom[i] = tempRightBroom[i-1];
		}
		
		float sumAreaLeft = 0.0001f; // prevent zero division
		float sumAreaCenter = 0.0001f; // prevent zero division
		float sumAreaRight = 0.0001f; // prevent zero division
		for ( int i = 0; i < history; i++ ) {
			sumAreaLeft = sumAreaLeft + tempLeftBroom[i];
			sumAreaCenter = sumAreaCenter + tempCenterBroom[i];
			sumAreaRight = sumAreaRight + tempRightBroom[i];
		}
		if (sumAreaLeft > 30) {
			sumAreaLeft = 30;
		}
		if (sumAreaCenter > 30) {
			sumAreaCenter = 30;
		}
		if (sumAreaRight > 30) {
			sumAreaRight = 30;
		}
		// sumArea varies now between 0 and 30
		leftFriction = sumAreaLeft * broomingCoefficient;
		centerFriction = sumAreaCenter * broomingCoefficient;
		rightFriction = sumAreaRight * broomingCoefficient;
		
		
		//Debug.Log ("sum of area: " + sumArea);
	}
}
