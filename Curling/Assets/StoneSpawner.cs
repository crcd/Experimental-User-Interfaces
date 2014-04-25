using UnityEngine;
using System.Collections;

public class StoneSpawner : MonoBehaviour {
	public GameObject stone;
	private ThrowerController throwerController;
	private BroomerController broomerController;
	private bool redTurn = false;
	public Mesh yellowMesh;
	public Mesh redMesh;
	
	void Start () {
		this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> (); 
		this.broomerController = GameObject.Find ("Broomer").GetComponent<BroomerController> (); 
		//this.spawnNewStone ();
	}
	
	void Update () {
		if (Input.GetKeyUp ("s")) {
			this.spawnNewStone ();
		}
	}
	
	GameObject spawnStone () {
		return Instantiate (stone, new Vector3(0, 1f, 0), new Quaternion(0, 0, 0, 0)) as GameObject;
	}
	
	private Rigidbody spawnYellowStone () {
		GameObject stone = this.spawnStone ();
		stone.GetComponent<MeshFilter> ().mesh = this.yellowMesh;
		stone.tag = "YellowStone";
		return stone.rigidbody;
	}
	
	private Rigidbody spawnRedStone () {
		GameObject stone = this.spawnStone ();
		stone.GetComponent<MeshFilter> ().mesh = this.redMesh;
		stone.tag = "RedStone";
		return stone.rigidbody;
	}
	
	public void spawnNewStone () {
		Rigidbody stoneBody;
		if (this.redTurn) {
			stoneBody = this.spawnRedStone ();
			this.redTurn = false;
		} else {
			stoneBody = this.spawnYellowStone ();
			this.redTurn = true;
		}
		this.throwerController.setStone (stoneBody);
		this.throwerController.resetToStartPosition ();
		this.broomerController.setStone (stoneBody);
		this.broomerController.resetToStartPosition ();
	}
}
