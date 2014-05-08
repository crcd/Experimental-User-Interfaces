using UnityEngine;
using System.Collections;

public class WallHitRemoval : MonoBehaviour {
    private GameLogic gameLogic;

    void Start () {
        this.gameLogic = GameObject.Find ("GameLogic").GetComponent<GameLogic> ();
    }

    bool isWallHit (Collision collision) {
        return collision.gameObject.layer == LayerMask.NameToLayer ("Walls");
    }

    bool isMovingStone () {
        return gameObject.tag == "MovingRedStone" || gameObject.tag == "MovingYellowStone";
    }

    void OnCollisionEnter (Collision collision) {
        if (isWallHit (collision)) {
            gameObject.rigidbody.velocity = new Vector3 (0, 0, 0);
            if (isMovingStone ()) {
                gameObject.tag = "";
                this.gameLogic.endThrow ();
            }
            Destroy (gameObject, 0.2f);
        }
    }
}
