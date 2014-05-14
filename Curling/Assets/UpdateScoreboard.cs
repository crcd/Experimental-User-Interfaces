using UnityEngine;
using System.Collections;

public class UpdateScoreboard : MonoBehaviour {
	
    public void UpdateBlueTeamScore(int newScore){
		GameObject scoreYellow = GameObject.Find ("ScoreYellow");
		if (scoreYellow) scoreYellow.GetComponent<TextMesh> ().text = newScore.ToString();
	}

    public void UpdateRedTeamScore(int newScore){
		GameObject scoreRed = GameObject.Find ("ScoreRed");
		if (scoreRed) scoreRed.GetComponent<TextMesh> ().text = newScore.ToString();
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
			GameObject yellowBall = GameObject.Find (stoneID);
            if (i <= stones) {

				if (yellowBall) yellowBall.renderer.enabled = true;
			} else {
				if (yellowBall) yellowBall.renderer.enabled = false;
			}
        }
		return;
    }

    public void SetCenterText(string text){
		GameObject guiText = GameObject.Find ("GUI_TEXT");
		if (guiText) guiText.GetComponent<TextMesh> ().text = text;
	}
}
