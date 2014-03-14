using UnityEngine;
[AddComponentMenu("VelocityAdderCs")]
public class VelocityAdderCs : MonoBehaviour
{
  public float velocity;
  public float angularVelocity;
  public void FixedUpdate() {
    if (Input.GetKeyDown ("space")) {
      rigidbody.velocity = new Vector3(0,0,velocity);
      rigidbody.angularVelocity = new Vector3(0,angularVelocity,0);
    }
  }
}
