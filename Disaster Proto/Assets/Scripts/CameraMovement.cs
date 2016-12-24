using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public GameObject robot;
	private Transform robotTrans;
	private PlayerMovement robotScript;

	// Use this for initialization
	void Start () {

		robotTrans = robot.GetComponent<Transform> ();
		robotScript = robot.GetComponent<PlayerMovement> ();

	}
	
	// Update is called once per frame
	void Update () {

		// pitch can range from 60 to 40.  the robot's height above the ground changes this value.
		float pitch = Mathf.Max (60 - 20f * (robotScript.GetHeight () / 40), 40f);
		// yaw can range from -15 to +15.  the robot's angular velocity changes this value.
		float yaw = 0f;
		if (robotScript.AngularVelocity < 0f) {
			// the (+) assignment makes the camera lead the robot while turning, and the (-) assignment makes it lag behind
			yaw = Mathf.Max (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), -15f);
//			yaw = -Mathf.Max (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), -15f);
		} else if (robotScript.AngularVelocity > 0f) {
			// the (+) assignment makes the camera lead the robot while turning, and the (-) assignment makes it lag behind
			yaw = Mathf.Min (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), 15f);
//			yaw = -Mathf.Min (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), 15f);
		} else {
			yaw = 0f;
		}
		// roll can range from -30 to 30.  the robot's own roll changes this value.
		float roll = 0f;
		// lerp the camera's angle based on the pitch, yaw, and roll calculated above
		transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (pitch, yaw, roll), Time.deltaTime * 2);
//		transform.Rotate (0f, 0f, roll);

		// sink can range from 15.0 to 7.5.  the robot's height above the ground changes this angle from 0-40m.
		float sink = Mathf.Max (15 - 7.5f * (robotScript.GetHeight () / 40), 7.5f);
		transform.localPosition = Vector3.Lerp (transform.localPosition, new Vector3 (0f, sink, -3f), Time.deltaTime);

//		Debug.Log (robotTrans.localRotation.z);
//		Debug.Log("pitch:\t" + pitch);						// show pitch angle
//		Debug.Log("yaw:\t" + yaw);							// show yaw angle
//		Debug.Log("roll:\t" + roll);						// show roll angle
//		Debug.Log("sink:\t" + sink);						// show sink level


	}
}
