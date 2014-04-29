using UnityEngine;
using System.Collections;

public class ClosestStone : MonoBehaviour {
    Vector3 centerPosition;

    void Start () {
        this.centerPosition = GameObject.Find ("CenterPosition").transform.position;
    }

    void Update () {
        Debug.Log (getCloserTeam ());
    }

    string getCloserTeam () {
        GameObject[] yellowStones = GameObject.FindGameObjectsWithTag ("ThrowedYellowStone");
        GameObject[] redStones = GameObject.FindGameObjectsWithTag ("ThrowedRedStone");
        float redTeamDistance = getClosestDistance (redStones);
        float yellowTeamDistance = getClosestDistance (yellowStones);
        if (redTeamDistance < yellowTeamDistance) {
            return "Red leads";
        } else {
            if (float.IsInfinity (yellowTeamDistance)) {
                return "No stones throwed";
            } else {
                return "Yellow leads";
            }
        }
    }

    float getClosestDistance (GameObject[] stones) {
        float smallestDistance = float.PositiveInfinity;
        foreach (GameObject stone in stones) {
            float distance = getDistanceFromCenter (stone);
            if (distance < smallestDistance) {
                smallestDistance = distance;
            }
        }
        return smallestDistance;
    }

    float getDistanceFromCenter (GameObject stone) {
        float xDistance = stone.rigidbody.position.x - this.centerPosition.x;
        float zDistance = stone.rigidbody.position.z - this.centerPosition.z;
        return Mathf.Sqrt (xDistance * xDistance + zDistance * zDistance);
    }
}
