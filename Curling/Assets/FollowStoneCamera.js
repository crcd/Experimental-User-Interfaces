#pragma strict

var offset : Vector3;

function Start () {


}

function Update () {

	var stone = GameObject.Find('StoneRed');
	gameObject.transform.position = stone.transform.position + offset;

}