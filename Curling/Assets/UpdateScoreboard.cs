using UnityEngine;
using System.Collections;

public class UpdateScoreboard : MonoBehaviour {

	void Start () {
        this.ResetScoreboard ();
	}

    public void ResetScoreboard() {
        this.SetCenterText ("vs");
        this.SetRedStonesLeft (8);
        this.SetYellowStonesLeft (8);
    }
	
    public void UpdateBlueTeamScore(int newScore){
		GameObject.Find ("ScoreBlue").GetComponent<GUIText> ().text = newScore.ToString();
	}

    public void UpdateRedTeamScore(int newScore){
		GameObject.Find ("ScoreRed").GetComponent<GUIText> ().text = newScore.ToString();
	}
       

    public void SetRedStonesLeft(int stones) {
        SetTeamStones ("yellow_ball_red", stones);
    }

    public void SetYellowStonesLeft(int stones) {
        SetTeamStones ("yellow_ball_blue", stones);
    }

    void SetTeamStones(string id, int stones) {

		// TODO: Fix me by cloning the guiTexture in StoneIndicator
		return;

//        for(int i = 1 ; i < 9 ; i++){
//            string stoneID = id + i.ToString ();
//            if (i <= stones)
//                GameObject.Find (stoneID).guiTexture.enabled = true;
//            else
//                GameObject.Find (stoneID).guiTexture.enabled = false;
//        }
    }

    void SetCenterText(string text){
        GameObject.Find ("GUI_Text").GetComponent<GUIText> ().text = text;
	}
}
