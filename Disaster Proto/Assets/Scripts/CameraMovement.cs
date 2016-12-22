using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public GameObject robot;
	PlayerMovement robotScript;

	// Use this for initialization
	void Start () {
		robot = GameObject.Find ("Robot");
		robotScript = robot.GetComponent<PlayerMovement> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
