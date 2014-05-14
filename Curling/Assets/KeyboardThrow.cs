using UnityEngine;
using System.Collections;

public class KeyboardThrow : MonoBehaviour {
    public Vector3 slidingScale;
    public float rotation;
    private ThrowerController throwerController;
    public CharacterCrouch characterCrouch;
    private GameLogic gameLogic;
    private bool initialized;
    private bool wasPressed;

    void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
        this.gameLogic = GameObject.Find ("GameLogic").GetComponent<GameLogic> ();
        this.initialized = false;
        this.wasPressed = false;
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
            this.initialized = true;
        }
        if (this.initialized) {
            this.throwerController.sawStone (Vector3.zero);
            if (this.isPressed ()) {
                this.throwerController.startSlidingToScale (slidingScale);
                this.wasPressed = true;
                if (characterCrouch)
                    characterCrouch.toggleCrouch (true);
            } else if (wasPressed && (isStoneOverThrowLine() || this.isReleased ())) {
                this.throwerController.throwStone (new Vector3 (0, 0, 0), rotation);
                this.initialized = false;
                this.wasPressed = false;
            }
        }
    }

    bool isStoneOverThrowLine () {
        if (throwerController.getStone ())
            return throwerController.getStone ().transform.position.z > -11f;
        return false;
    }
}
