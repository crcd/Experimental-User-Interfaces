using UnityEngine;
using System.Collections;

public class UpdateScoreboard : MonoBehaviour {

	int redScore; // only for testing TODO: delete this
	int blueScore; // only for testing TODO: delete this
	int roundCount; // only for testing TODO: delete this

	int stoneLeftBlueTeam;
	int stoneLeftRedTeam;
	string[] roundArray;
	// Use this for initialization
	void Start () {

		// only for testing TODO: delete this
		this.redScore = 0;
		this.blueScore = 0;
		this.roundCount = 0;


		this.stoneLeftBlueTeam = 8;
		this.stoneLeftRedTeam = 8;
		this.roundArray = new string[11];
		this.roundArray [0] = "vs";
		this.roundArray [1] = "1st end";
		this.roundArray [2] = "2nd end";
		this.roundArray [3] = "3rd end";
		this.roundArray [4] = "4th end";
		this.roundArray [5] = "5th end";
		this.roundArray [6] = "6th end";
		this.roundArray [7] = "7th end";
		this.roundArray [8] = "8th end";
		this.roundArray [9] = "Blue Won!";
		this.roundArray [10] = "Red Won!";

	
	}
	
	// Update is called once per frame
	void Update () {

		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.B)) {
			this.blueScore++;
			if(this.blueScore > 99){
				this.blueScore = 0;
			}
			this.UpdateBlueTeamScore(this.blueScore);
		}
		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.R)) {
			this.redScore++;
			if(this.redScore > 99){
				this.redScore = 0;
			}
			this.UpdateRedTeamScore(this.redScore);
		}

		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.W)) {
			if(this.stoneLeftBlueTeam == 0){
				this.ResetTeamsStones();
			}
			else{
				this.DeleteOneUnusedStoneBlueTeam();
			}
		}

		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.Q)) {
			if(this.stoneLeftRedTeam == 0){
				this.ResetTeamsStones();
			}
			else{
				this.DeleteOneUnusedStoneRedTeam();
			}
		}

		// only for testing TODO: delete this
		if (Input.GetKeyUp(KeyCode.E)) {
			this.roundCount++;
			if(this.roundCount > 10){
				this.roundCount = 0;
			}
			this.UpdateRound(this.roundCount);
		}
	}

	void UpdateBlueTeamScore(int newScore){
		GameObject.Find ("ScoreBlue").GetComponent<GUIText> ().text = newScore.ToString();
	}

	void UpdateRedTeamScore(int newScore){
		GameObject.Find ("ScoreRed").GetComponent<GUIText> ().text = newScore.ToString();
	}

	void DeleteOneUnusedStoneBlueTeam(){
		string stoneID = "yellow_ball_blue" + this.stoneLeftBlueTeam.ToString ();
		GameObject selectedYellowBall = GameObject.Find (stoneID);
		selectedYellowBall.guiTexture.enabled = false;
		this.stoneLeftBlueTeam--;
	}

	void DeleteOneUnusedStoneRedTeam(){
		string stoneID = "yellow_ball_red" + this.stoneLeftRedTeam.ToString ();
		GameObject selectedYellowBall = GameObject.Find (stoneID);
		selectedYellowBall.guiTexture.enabled = false;
		this.stoneLeftRedTeam--;
	}

	void ResetTeamsStones(){
		for(int i = 1 ; i < 9 ; i++){
			Debug.Log(i.ToString());
			string stoneIDBlueTeam = "yellow_ball_blue" + i.ToString();
			string stoneIDRedTeam = "yellow_ball_red" + i.ToString();
			Debug.Log(stoneIDBlueTeam);
			Debug.Log(stoneIDRedTeam);
			GameObject selectedYellowBallBlueTeam = GameObject.Find (stoneIDBlueTeam);
			GameObject selectedYellowBallRedTeam = GameObject.Find (stoneIDRedTeam);
			selectedYellowBallBlueTeam.guiTexture.enabled = true;
			selectedYellowBallRedTeam.guiTexture.enabled = true;
			
		}
		this.stoneLeftBlueTeam = 8;
		this.stoneLeftRedTeam = 8;
	}

	void UpdateRound(int index){
		if(index > -1 && index < 12){
			GameObject.Find ("GUI_Text").GetComponent<GUIText> ().text = this.roundArray[index];
		}

	}
}
