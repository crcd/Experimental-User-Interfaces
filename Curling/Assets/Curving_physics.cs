using UnityEngine;
using System.Collections;

public class Curving_physics : MonoBehaviour {
  public float curve_amount;
  public void FixedUpdate() {
    rigidbody.AddForce(
      new Vector3(rigidbody.angularVelocity.y*curve_amount,0,0)
    );
  }
}
