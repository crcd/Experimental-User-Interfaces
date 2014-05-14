using UnityEngine;
using System.Collections;

public class PantsColor : MonoBehaviour {

	private bool redPants;
	private GameLogic gameLogic;
	
	private SkinnedMeshRenderer meshRenderer;
	public Material yellowMaterial;
	public Material redMaterial;

	// Use this for initialization
	void Start () {

		this.gameLogic = GameObject.Find ("GameLogic").GetComponent<GameLogic>();
		this.redPants = this.gameLogic.isRedTurn ();
		this.meshRenderer = gameObject.GetComponent<SkinnedMeshRenderer> ();

	}
	
	// Update is called once per frame
	void Update () {

		Debug.Log (this.redPants);

		if (this.gameLogic.isRedTurn ()) {
			if (!this.redPants) {
				meshRenderer.material = this.redMaterial;
				this.redPants = true;
			} 
		} else {
			if (this.redPants) {
				meshRenderer.material = this.yellowMaterial;
				this.redPants = false;
			}
		}

	}
}
