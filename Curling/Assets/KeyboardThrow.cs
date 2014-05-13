using UnityEngine;
using System.Collections;

public class KeyboardThrow : MonoBehaviour {
    public Vector3 slidingScale;
    public float rotation;
    private ThrowerController throwerController;
    public CharacterCrouch characterCrouch;
    private GameLogic gameLogic;

    void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
        this.gameLogic = GameObject.Find ("GameLogic").GetComponent<GameLogic> ();
    }

    bool isPressed () {
        return Input.GetKeyDown ("space");
    }

    bool isReleased () {
        return Input.GetKeyUp ("space");
    }

    void Update () {
        if (Input.GetKeyUp ("r"))
            gameLogic.resetRound ();
        if (Input.GetKeyUp ("s")) {
            this.gameLogic.startNewThrow ();
        }
        if (this.isPressed ()) {
            this.throwerController.startSlidingToScale (slidingScale);
            if (characterCrouch)
                characterCrouch.toggleCrouch (true);
        } else if (this.isReleased ()) {
            this.throwerController.throwStone (new Vector3 (0, 0, 0), rotation);
        }
    }
}
