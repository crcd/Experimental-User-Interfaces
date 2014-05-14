using UnityEngine;
using System.Collections;

public class crowdSoundController : MonoBehaviour {
	
	public AudioClip clipAmbience1;
	public AudioClip clipLaugh;
	public AudioClip clipLaugh2;
	public AudioClip clipApplause1;
	public AudioClip clipApplause2;

	private AudioSource audioAmbience1;
	private AudioSource audioLaugh;
	private AudioSource audioLaugh2;
	private AudioSource audioApplause1;
	private AudioSource audioApplause2;

	public bool applauding;
	
	AudioSource AddAudio (AudioClip clip, bool loop, bool playOnAwake, float volume) {
		AudioSource newAudio = gameObject.AddComponent<AudioSource>();
		newAudio.clip = clip;
		newAudio.loop = loop;
		newAudio.playOnAwake = playOnAwake;
		newAudio.volume = volume;
		return newAudio;
	}
	
	void Awake () {
		audioAmbience1 = AddAudio (clipAmbience1, true, true, 1.0f);
		audioLaugh = AddAudio (clipLaugh, false, false, 1.0f);
		audioLaugh2 = AddAudio (clipLaugh2, false, false, 1.0f);
		audioApplause1 = AddAudio (clipApplause1, false, false, 1.0f);
		audioApplause2 = AddAudio (clipApplause2, false, false, 1.0f);
	}
	
	/*void OnCollisionEnter(Collision collision) {
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
		
	}*/
	
	// Use this for initialization
	void Start () {
		gameObject.GetComponent<crowdSoundController>().applauding = false;
		//audioStoneHitStone.Play ();
		audioAmbience1.Play ();
	}

	public void playApplause() {
		if (audioLaugh.isPlaying == false && audioLaugh2.isPlaying == false) {
			if (gameObject.GetComponent<crowdSoundController>().applauding == false) {
				gameObject.GetComponent<crowdSoundController>().applauding = true;
				float rnd = Random.Range (0.0f, 30.0f);
				if ( rnd >= 15.0f) {
					audioApplause1.Play ();
				} else {
					audioApplause2.Play ();
				}
			}
		}
	}

	public void playLaugh() {
		float rnd = Random.Range (-10.0f, 10.0f);
		if (rnd <= 2.0f) {
			audioLaugh2.Play ();
		} else {
			audioLaugh.Play();
		}
	}

	// Update is called once per frame
	// Play ambience sounds randomly, start after sound finishes (how to check that?)

	void Update () {
		if (audioApplause1.isPlaying == false && audioApplause2.isPlaying == false) {
			gameObject.GetComponent<crowdSoundController>().applauding = false;
		}
		//Debug.Log (audioAmbience1.time + "/" + audioAmbience1.clip.length);
		/*float x = Mathf.Abs (gameObject.rigidbody.velocity.z);
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
		}*/
	}
}
