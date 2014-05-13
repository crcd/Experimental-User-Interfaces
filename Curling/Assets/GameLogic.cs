using UnityEngine;
using System.Collections;
using System.Linq;

public class GameLogic : MonoBehaviour {
    private StoneSpawner stoneSpawner;
    private bool redTurn = false;
    private UpdateScoreboard scoreBoard;
    private ClosestStone closestStoneCalculator;
    private int redStonesLeft;
    private int yellowStonesLeft;
    private StoneRemover stoneRemover;
    private StoneFinder stoneFinder;
    // Use this for initialization
    void Start () {
        this.stoneSpawner = GameObject.Find ("GameLogic").GetComponent<StoneSpawner> ();
        this.scoreBoard = gameObject.AddComponent<UpdateScoreboard> ();
        this.closestStoneCalculator = gameObject.AddComponent<ClosestStone> ();
        this.stoneRemover = gameObject.AddComponent<StoneRemover> ();
        this.stoneFinder = gameObject.AddComponent<StoneFinder> ();
        this.resetRound ();
    }

    void resetRound () {
        this.redStonesLeft = 8;
        this.yellowStonesLeft = 8;
        this.scoreBoard.ResetScoreboard ();
    }

    void updateScoreBoard () {
        this.scoreBoard.SetRedStonesLeft (this.redStonesLeft);
        this.scoreBoard.SetYellowStonesLeft (this.yellowStonesLeft);
        this.scoreBoard.UpdateRedTeamScore (this.closestStoneCalculator.getRedTeamPoints ());
        this.scoreBoard.UpdateBlueTeamScore (this.closestStoneCalculator.getYellowTeamPoints ());
    }

    public void startNewThrow () {
        this.spawnNewStone ();
        //spawn
    }

    public void endThrow () {
        this.spawnNewStone ();
    }

    bool isAnyStoneMoving () {
        GameObject[] stones = stoneFinder.getAllStones ();
        foreach (GameObject stone in stones) {
            if (Vector3.Distance (stone.rigidbody.velocity, Vector3.zero) > 0.05) {
                return true;
            }
        }
        return false;
    }

    bool isThereACurrentStone () {
        return (
            GameObject.FindGameObjectsWithTag ("CurrentYellowStone").Length > 0
            || GameObject.FindGameObjectsWithTag ("CurrentRedStone").Length > 0
        );
    }

    bool isItFreeToSpawn () {
        return !isThereACurrentStone () && !isAnyStoneMoving ();
    }

    void spawnNewStone () {
        if (isItFreeToSpawn ()) {
            GameObject stone;
            if (this.redTurn) {
                stone = this.stoneSpawner.spawnRedStone ();
                this.redTurn = false;
                this.redStonesLeft--;
            } else {
                stone = this.stoneSpawner.spawnYellowStone ();
                this.redTurn = true;
                this.yellowStonesLeft--;
            }
        }
        updateScoreBoard ();
    }

    GameObject findMovingStone () {
        GameObject movingStone = GameObject.FindGameObjectWithTag ("MovingYellowStone");
        if (movingStone == null)
            movingStone = GameObject.FindGameObjectWithTag ("MovingRedStone");
        return movingStone;
    }
    // Update is called once per frame
    void Update () {
        if (!isAnyStoneMoving ()) {
            updateScoreBoard ();
            stoneRemover.removeTooShortStones ();
        }
    }
}
