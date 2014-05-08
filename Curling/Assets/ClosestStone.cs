using UnityEngine;
using System.Collections;
using System.Linq;

public class ClosestStone : MonoBehaviour {
    Vector3 centerPosition;

    void Start () {
        this.centerPosition = GameObject.Find ("CenterPosition").transform.position;
    }

    public int getRedTeamPoints () {
        int status = getStatus ();
        if (status > 0) {
            return status;
        } else {
            return 0;
        }
    }

    public int getYellowTeamPoints () {
        int status = getStatus ();
        if (status < 0) {
            return -status;
        } else {
            return 0;
        }
    }

    int getStatus() {
        GameObject[] stones = getThrowedStonesSorted ();
        string winnerTag = "";
        int winningStones = 0;
        for (int i = 0; i < stones.Length; i++) {
            if (i == 0) {
                winnerTag = stones [i].tag;
            }
            if (stones [i].tag == winnerTag) {
                winningStones++;
            } else {
                break;
            }
        }
        if (winnerTag == "ThrowedRedStone") {
            return winningStones;
        } else {
            return -winningStones;
        }
    }

    GameObject[] getThrowedStonesSorted () {
        GameObject[] stones = GameObject.FindGameObjectsWithTag ("ThrowedYellowStone");
        stones = stones.Concat (GameObject.FindGameObjectsWithTag ("ThrowedRedStone")).ToArray ();
        return stones.OrderBy (stone => getDistanceFromCenter ((GameObject)stone)).ToArray();
    }

    float getDistanceFromCenter (GameObject stone) {
        float xDistance = stone.rigidbody.position.x - this.centerPosition.x;
        float zDistance = stone.rigidbody.position.z - this.centerPosition.z;
        return Mathf.Sqrt (xDistance * xDistance + zDistance * zDistance);
    }
}
