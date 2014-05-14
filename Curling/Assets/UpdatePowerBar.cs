using UnityEngine;
using System.Collections;

public class UpdatePowerBar : MonoBehaviour {
    private float minPosition = 3f;
    private float maxPosition = 79f;

    public void SetPowerPercentage (float percentage) {

		Debug.Log (percentage);
//        GUITexture arrowTexture = GameObject.Find ("Arrow").guiTexture;
//        Rect newInset = new Rect(
//            arrowTexture.pixelInset.x,
//            minPosition + (this.maxPosition - this.minPosition) * percentage / 100.0f,
//            arrowTexture.pixelInset.width,
//            arrowTexture.pixelInset.height
//        );
//        arrowTexture.pixelInset = newInset;
		GameObject arrow = GameObject.Find ("Arrow");
		Vector3 pos = arrow.transform.localPosition;

		arrow.transform.localPosition = new Vector3 (
			pos.x,
			pos.y,
			-((percentage / 100.0f) * (2 * 0.65f) - 0.65f)
		);
    }
}
