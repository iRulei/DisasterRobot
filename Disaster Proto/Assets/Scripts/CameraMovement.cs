using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public GameObject robot;
	private PlayerMovement robotScript;

	// Use this for initialization
	void Start () {
		
		robotScript = robot.GetComponent<PlayerMovement> ();

	}
	
	// Update is called once per frame
	void Update () {

		// angle can range from 60 to 40.  the robot's height above the ground changes this angle from 0-40m.
		float angle = Mathf.Max (60 - 20f * (robotScript.GetHeight () / 40), 40f);
		transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (angle, 0f, 0f), Time.deltaTime);

		// sink can range from 15.0 to 7.5.  the robot's height above the ground changes this angle from 0-40m.
		float sink = Mathf.Max (15 - 7.5f * (robotScript.GetHeight () / 40), 7.5f);
		transform.localPosition = Vector3.Lerp (transform.localPosition, new Vector3 (0f, sink, -3f), Time.deltaTime);

	}
}
