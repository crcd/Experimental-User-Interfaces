using UnityEngine;
using System.Collections;

public class StoneSoundController : MonoBehaviour {

	public AudioClip clipStoneHitStone;
	public AudioClip clipStoneCurl;
	public AudioClip clipStoneHitWall;

	private AudioSource audioStoneHitStone;
	private AudioSource audioStoneCurl;
	private AudioSource audioStoneHitWall;

	public bool stationary;

	AudioSource AddAudio (AudioClip clip, bool loop, bool playOnAwake, float volume) {
		AudioSource newAudio = gameObject.AddComponent<AudioSource>();
		newAudio.clip = clip;
		newAudio.loop = loop;
		newAudio.playOnAwake = playOnAwake;
		newAudio.volume = volume;
		return newAudio;
	}

	void Awake () {
		audioStoneHitStone = AddAudio (clipStoneHitStone, false, false, 1.0f);
		audioStoneCurl = AddAudio (clipStoneCurl, true, false, 1.0f);
		audioStoneHitWall = AddAudio (clipStoneHitWall, false, false, 1.0f);
	}
	
	void OnCollisionEnter(Collision collision) {
		// We can't test against prefab's type, because it's also a gameObject
		// therefore we need to use the names.. StoneRed / StoneYellow..
		// maybe we could test also part of the name..
		if (collision.gameObject.name.Contains("Stone")) {
			audioStoneHitStone.pitch = gameObject.rigidbody.velocity.sqrMagnitude;
			if (audioStoneHitStone.pitch < 0.8) {
				audioStoneHitStone.pitch = 0.8f;
			} else if (audioStoneHitStone.pitch > 1.5) {
				audioStoneHitStone.pitch = 1.5f;
			}
			audioStoneHitStone.Play ();
		}

		if (collision.gameObject.name == "SheetBackBound" ||
		    collision.gameObject.name == "SheetLeftBound" ||
		    collision.gameObject.name == "SheetRightBound" ||
		    collision.gameObject.name == "SheetFrontBound") {
			audioStoneHitWall.Play ();
		}

	}

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<StoneSoundController>().stationary = true;
		//audioStoneHitStone.Play ();
	}
	
	// Update is called once per frame
	// Play one looping stone sound for each moving gameObject stone.
	void Update () {
		float x = Mathf.Abs (gameObject.rigidbody.velocity.z);
		float xx = Mathf.Abs (gameObject.rigidbody.velocity.x);
		if (xx > x) {
			x = xx;
		}
		//float x = gameObject.rigidbody.velocity.sqrMagnitude;
		if (x > 0.1) {
			if (gameObject.GetComponent<StoneSoundController> ().stationary) {
				audioStoneCurl.Play ();
			} else {
				// use tanh for stone pitch: (0.5*tanh((-2+x)*0.5)+1.2)
				// absolute value of z (for negative momvement too)
				// (0.1(7*Exp(2-x) + 17)) / (Exp(2-x)+1)
				float y = (0.1f*(7.0f * Mathf.Exp(2.0f - x) + 17.0f)) / (Mathf.Exp(2.0f - x)+1.0f);
				audioStoneCurl.pitch = y;
				if (x > 2.0) {
					audioStoneCurl.volume = 1.0f;
				} else {
					audioStoneCurl.volume = 0.5f * x;
				}
			}
			gameObject.GetComponent<StoneSoundController>().stationary = false;

		} else {
			audioStoneCurl.Stop ();
			gameObject.GetComponent<StoneSoundController>().stationary = true;
		}
	}
}
