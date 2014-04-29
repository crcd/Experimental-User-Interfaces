using UnityEngine;
using System.Collections;

public class WallHitRemoval : MonoBehaviour {
    private StoneSpawner stoneSpawner;

    void Start () {
        this.stoneSpawner = GameObject.Find ("GameLogic").GetComponent<StoneSpawner> ();
    }

    bool isWallHit (Collision collision) {
        return collision.gameObject.layer == LayerMask.NameToLayer ("Walls");
    }

    bool isMovingStone() {
        return gameObject.tag == "MovingRedStone" || gameObject.tag == "MovingYellowStone";
    }

    void OnCollisionEnter (Collision collision) {
        if (isWallHit (collision)) {
            gameObject.rigidbody.velocity = new Vector3 (0, 0, 0);
            if (isMovingStone ()) {
                gameObject.tag = "";
                this.stoneSpawner.spawnNewStone ();
            }
            Destroy (gameObject, 0.2f);
        }
    }
}
