using UnityEngine;
using System.Collections;
using System.Linq;

public class StoneFinder : MonoBehaviour {
    public GameObject[] getAllStones () {
        GameObject[] stones = GameObject.FindGameObjectsWithTag ("MovingRedStone");
        stones = stones.Concat (GameObject.FindGameObjectsWithTag ("MovingYellowStone")).ToArray ();
        stones = stones.Concat (GameObject.FindGameObjectsWithTag ("ThrowedRedStone")).ToArray ();
        stones = stones.Concat (GameObject.FindGameObjectsWithTag ("ThrowedYellowStone")).ToArray ();
        return stones;
    }

}
