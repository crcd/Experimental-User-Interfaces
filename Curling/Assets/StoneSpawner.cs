using UnityEngine;
using System.Collections;

public class StoneSpawner : MonoBehaviour {
    public GameObject stone;
    private ThrowerController throwerController;
    private bool redTurn = false;
    public Mesh yellowMesh;
    public Mesh redMesh;

    void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> (); 
        this.spawnNewStone ();
    }

    void Update () {
        if (Input.GetKeyUp ("s")) {
            this.spawnNewStone ();
        }
    }

    GameObject spawnStone () {
        return Instantiate (stone) as GameObject;
    }

    private Rigidbody spawnYellowStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.yellowMesh;
        return stone.rigidbody;
    }

    private Rigidbody spawnRedStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.redMesh;
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
    }
}
