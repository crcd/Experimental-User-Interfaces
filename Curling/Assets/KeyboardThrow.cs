using UnityEngine;
using System.Collections;

public class KeyboardThrow : MonoBehaviour {
    public Vector3 slidingForce;
    public float rotation;
    private ThrowerController throwerController;
	public CharacterCrouch characterCrouch; 
	// Use this for initialization
	void Start () {
        this.throwerController = GameObject.Find ("Thrower").GetComponent<ThrowerController> ();
	}

    bool isPressed () {
        return Input.GetKeyDown ("space");
    }

    bool isReleased () {
        return Input.GetKeyUp ("space");
    }
	
    void Update () {
        if (this.isPressed ()) {
			this.throwerController.startSliding (slidingForce);
			if (characterCrouch) characterCrouch.toggleCrouch(true);
        } else if (this.isReleased ()) {
            this.throwerController.throwStone (new Vector3 (0, 0, 0), rotation);
        }
    }
}
