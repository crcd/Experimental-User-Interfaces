using UnityEngine;
using System.Collections;

public class UpdatePowerBar : MonoBehaviour {
    private float minPosition = 3f;
    private float maxPosition = 79f;

    public void SetPowerPercentage (float percentage) {
        GUITexture arrowTexture = GameObject.Find ("Arrow").guiTexture;
        Rect newInset = new Rect(
            arrowTexture.pixelInset.x,
            minPosition + (this.maxPosition - this.minPosition) * percentage / 100.0f,
            arrowTexture.pixelInset.width,
            arrowTexture.pixelInset.height
        );
        arrowTexture.pixelInset = newInset;
    }
}
