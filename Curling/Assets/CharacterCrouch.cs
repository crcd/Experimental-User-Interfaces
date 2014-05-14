using UnityEngine;
using System.Collections;

public class CharacterCrouch : MonoBehaviour 
{
	
	public Animator animator;
	private bool crouching;
	
	void Awake()
	{
//		if(!animator)
//			Debug.LogError(typeof(Animator).Name + " is not attached to " + this.name + " script!");
//		animator.SetBool("Crouching", false);
//		crouching = false;

	}

	void Start() {
		animator.SetBool("Crouching", true);
	}
	
	bool keyDown () {
		return Input.GetKeyDown (KeyCode.LeftControl);
	}
	
	bool keyUp () {
		return Input.GetKeyUp (KeyCode.LeftControl);
	}

	public void toggleCrouch(bool newVal) {
		crouching = newVal;
	}
	
	void Update () 
	{
		if (!animator) return;

//		if (crouching && keyUp()) {
//			crouching = !crouching;
//			animator.SetBool("Crouching", false);
//		}
//		if (!crouching && keyDown()) {
//			crouching = !crouching;
//			animator.SetBool("Crouching", true);
//		}
		//animator.SetBool("Crouching", true);
		
		
	}
}
