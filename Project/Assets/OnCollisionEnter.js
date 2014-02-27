#pragma strict

@script RequireComponent(AudioSource)
function OnCollisionEnter() {
  Debug.Log('taalla ollaan');
  audio.Play();
}

function Update () {

}