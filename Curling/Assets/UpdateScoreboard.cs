using UnityEngine;
using System.Collections;

public class UpdateScoreboard : MonoBehaviour {
	
    public void UpdateBlueTeamScore(int newScore){
		GameObject.Find ("ScoreYellow").GetComponent<TextMesh> ().text = newScore.ToString();
	}

    public void UpdateRedTeamScore(int newScore){
		GameObject.Find ("ScoreRed").GetComponent<TextMesh> ().text = newScore.ToString();
	}
       

    public void SetRedStonesLeft(int stones) {
        SetTeamStones ("RedBall", stones);
    }

    public void SetYellowStonesLeft(int stones) {
        SetTeamStones ("YellowBall", stones);
    }

    void SetTeamStones(string id, int stones) {
		
        for(int i = 1 ; i < 9 ; i++){
            string stoneID = id + i.ToString ();
            if (i <= stones)
                GameObject.Find (stoneID).renderer.enabled = true;
            else
                GameObject.Find (stoneID).renderer.enabled = false;
        }
		return;
    }

    public void SetCenterText(string text){
		GameObject.Find ("GUI_TEXT").GetComponent<TextMesh> ().text = text;
	}
}
