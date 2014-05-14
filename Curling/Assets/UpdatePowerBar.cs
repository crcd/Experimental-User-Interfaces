using UnityEngine;
using System.Collections;

public class UpdatePowerBar : MonoBehaviour {
    private float minPosition = 3f;
    private float maxPosition = 79f;

    public void SetPowerPercentage (float percentage) {

		GameObject arrow = GameObject.Find ("Arrow");
		Vector3 pos = arrow.transform.localPosition;

		arrow.transform.localPosition = new Vector3 (
			pos.x,
			pos.y,
			-((percentage / 100.0f) * (2 * 0.65f) - 0.65f)
		);
    }
}
