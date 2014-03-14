@script AddComponentMenu ("Physics/addVelocity")
class VelocityAdder extends MonoBehaviour {
  var k = 0;
  function Update () {
    if (Input.GetKeyDown ("space")) {
      rigidbody.velocity = Vector3(0,0,10);
      rigidbody.angularVelocity = Vector3(0,300,0);
      print ("space key was pressed");
    }
  }
}

