using UnityEngine;
using System.Collections;

public class StoneRemover : MonoBehaviour {
    private StoneFinder stoneFinder;

    void Start () {
        this.stoneFinder = gameObject.AddComponent<StoneFinder> ();
    }

    public void removeTooShortStones () {
        GameObject[] stones = stoneFinder.getAllStones ();
		bool destroyed = false;
		foreach (GameObject stone in stones) {
            if (isStoneTooShort (stone)) {
                destroyStone (stone);
				destroyed = true;
			}
        }
		if (destroyed) {
			GameObject.Find ("Stadium").GetComponent<crowdSoundController> ().playLaugh ();
		}
    }

    public void destroyStone (GameObject stone) {
        Destroy (stone, 0.2f);
    }

    bool isStoneTooShort (GameObject stone) {
        if (stone.transform.position.z <= 11f) {
            return true;
		} else {
            return false;
		}
    }
}
