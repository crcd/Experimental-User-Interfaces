using UnityEngine;
using System.Collections;

public class StoneSpawner : MonoBehaviour {
    public GameObject stoneObject;
    private ThrowerController throwerController;
    private BroomerController broomerController;
    private GameObject thrower;
    public Mesh yellowMesh;
    public Mesh redMesh;

    void Start () {
        this.thrower = GameObject.Find ("Thrower");
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> (); 
        this.broomerController = GameObject.Find ("Broomer").GetComponent<BroomerController> (); 
    }

    GameObject spawnStone () {
        return Instantiate (this.stoneObject, this.thrower.transform.position, new Quaternion (0, 0, 0, 0)) as GameObject;
    }

    public GameObject spawnYellowStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.yellowMesh;
        stone.tag = "CurrentYellowStone";
        initControllers (stone);
        return stone;
    }

    public GameObject spawnRedStone () {
        GameObject stone = this.spawnStone ();
        stone.GetComponent<MeshFilter> ().mesh = this.redMesh;
        stone.tag = "CurrentRedStone";
        initControllers (stone);
        return stone;
    }

    GameObject findMovingStone () {
        GameObject movingStone = GameObject.FindGameObjectWithTag ("MovingYellowStone");
        if (movingStone == null)
            movingStone = GameObject.FindGameObjectWithTag ("MovingRedStone");
        return movingStone;
    }

    void initControllers (GameObject stone) {
        this.throwerController.setStone (stone);
        this.throwerController.resetToStartPosition ();
        this.broomerController.setStone (stone.rigidbody);
        this.broomerController.resetToStartPosition ();
    }

    void Update () {
        GameObject stone = findMovingStone ();
        if (stone != null) {
            if (Vector3.Distance (stone.rigidbody.velocity, new Vector3 (0, 0, 0)) < 0.05) {
                if (stone.tag == "MovingRedStone")
                    stone.tag = "ThrowedRedStone";
                else if (stone.tag == "MovingYellowStone")
                    stone.tag = "ThrowedYellowStone";
            }
        }
    }
}
