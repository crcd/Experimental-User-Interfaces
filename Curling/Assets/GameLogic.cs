using UnityEngine;
using System.Collections;
using System.Linq;

public class GameLogic : MonoBehaviour {
    private bool redTurn = false;
    private int redStonesLeft;
    private int yellowStonesLeft;
    private StoneSpawner stoneSpawner;
    private UpdateScoreboard scoreBoard;
    private ClosestStone closestStoneCalculator;
    private StoneRemover stoneRemover;
    private StoneFinder stoneFinder;
    private bool matchInProgress;
    private Throw throwControl;
    // Use this for initialization
    void Start () {
        this.stoneSpawner = GameObject.Find ("GameLogic").GetComponent<StoneSpawner> ();
        this.scoreBoard = gameObject.AddComponent<UpdateScoreboard> ();
        this.closestStoneCalculator = gameObject.AddComponent<ClosestStone> ();
        this.stoneRemover = gameObject.AddComponent<StoneRemover> ();
        this.stoneFinder = gameObject.AddComponent<StoneFinder> ();
        if (GameObject.Find ("ThrowerPSWand"))
            this.throwControl = GameObject.Find ("ThrowerPSWand").GetComponent<PsMoveThrow> ();
        this.resetRound ();
    }

    public void resetRound () {
        GameObject[] stones = stoneFinder.getAllStones ();
        foreach (GameObject stone in stones) {
            DestroyImmediate (stone);
        }
        this.redStonesLeft = 1;
        this.yellowStonesLeft = 1;
        this.matchInProgress = true;
        this.updateScoreBoard ();
    }

    void updateScoreBoard () {
        this.scoreBoard.SetRedStonesLeft (this.redStonesLeft);
        this.scoreBoard.SetYellowStonesLeft (this.yellowStonesLeft);
        this.scoreBoard.UpdateRedTeamScore (this.closestStoneCalculator.getRedTeamPoints ());
        this.scoreBoard.UpdateBlueTeamScore (this.closestStoneCalculator.getYellowTeamPoints ());
    }

    public void startNewThrow () {
        this.spawnNewStone ();
		GameObject.Find ("Stadium").GetComponent<crowdSoundController> ().playApplause ();
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
        if (throwControl) {
            if (throwControl.isPressed ())
                return false;
        }
        return !isThereACurrentStone () && !isAnyStoneMoving ();
    }

    void spawnNewStone () {
        if (matchInProgress && isItFreeToSpawn ()) {
			GameObject.Find ("Stadium").GetComponent<crowdSoundController> ().playApplause ();
            if (this.redTurn) {
                this.stoneSpawner.spawnRedStone ();
                this.redTurn = false;
                this.redStonesLeft--;
                scoreBoard.SetCenterText ("Red turn");
            } else {
                this.stoneSpawner.spawnYellowStone ();
                this.redTurn = true;
                this.yellowStonesLeft--;
                scoreBoard.SetCenterText ("Yellow turn");
            }
        }
        updateScoreBoard ();
    }

    void endGame () {
        this.matchInProgress = false;
        if (this.closestStoneCalculator.getRedTeamPoints () > this.closestStoneCalculator.getYellowTeamPoints ())
            scoreBoard.SetCenterText ("Red wins");
        else if (this.closestStoneCalculator.getRedTeamPoints () == this.closestStoneCalculator.getYellowTeamPoints ())
            scoreBoard.SetCenterText ("TIE");
        else
            scoreBoard.SetCenterText ("Yellow wins");
		GameObject.Find ("Stadium").GetComponent<crowdSoundController> ().playApplause ();
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
            stoneRemover.removeTooShortStones ();
            updateScoreBoard ();
            if (!isThereACurrentStone () && redStonesLeft < 1 && yellowStonesLeft < 1) {
                endGame ();
            }
        }
    }
}
