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
    }

    GameObject spawnStone () {
        return Instantiate (stone, new Vector3 (0, 1f, 0), new Quaternion (0, 0, 0, 0)) as GameObject;
    }

    private GameObject spawnYellowStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.yellowMesh;
        stone.tag = "CurrentYellowStone";
        return stone;
    }

    private GameObject spawnRedStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.redMesh;
        stone.tag = "CurrentRedStone";
        return stone;
    }

    GameObject findMovingStone() {
        GameObject movingStone = GameObject.FindGameObjectWithTag ("MovingYellowStone");
        if (movingStone == null)
            movingStone = GameObject.FindGameObjectWithTag ("MovingRedStone");
        return movingStone;
    }

    bool isThereAMovingStone () {
        return findMovingStone() != null;
    }

    bool isThereACurrentStone () {
        return (
            GameObject.FindGameObjectsWithTag ("CurrentYellowStone").Length > 0
            || GameObject.FindGameObjectsWithTag ("CurrentRedStone").Length > 0
        );
    }

    bool isItFreeToSpawn () {
        return !isThereACurrentStone () && !isThereAMovingStone ();
    }

    public void spawnNewStone () {
        if (isItFreeToSpawn()) {
            GameObject stone;
            if (this.redTurn) {
                stone = this.spawnRedStone ();
                this.redTurn = false;
            } else {
                stone = this.spawnYellowStone ();
                this.redTurn = true;
            }
            this.throwerController.setStone (stone);
            this.throwerController.resetToStartPosition ();
            this.broomerController.setStone (stone.rigidbody);
            this.broomerController.resetToStartPosition ();
        }
    }

    void Update() {
        GameObject stone = findMovingStone ();
        if (stone != null) {
            if (Vector3.Distance (stone.rigidbody.velocity, new Vector3 (0, 0, 0)) < 0.05) {
                if (stone.tag == "MovingRedStone")
                    stone.tag = "ThrowedRedStone";
                else
                    stone.tag = "ThrowedYellowStone";
            }
        }
    }
}
