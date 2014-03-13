#pragma strict

@script RequireComponent(AudioSource)
function OnCollisionEnter() {
  audio.Play();
}

function Update () {

}