using UnityEngine;
using System.Collections;

public class SpacebarThrow : MonoBehaviour {
  public float velocity;
  public float angularVelocity;
  public void Update() {
    if (Input.GetKeyDown ("space")) {
      rigidbody.velocity = new Vector3(0,0,velocity);
      rigidbody.angularVelocity = new Vector3(0,angularVelocity,0);
    }
  }
}
