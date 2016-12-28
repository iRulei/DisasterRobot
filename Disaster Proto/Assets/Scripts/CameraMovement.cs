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

		// pitch can range from 40 to 70.  the robot's height above the ground changes this value.
		float pitch = Mathf.Min (40 + 30f * (robotScript.GetHeight () / 40), 70f);
		// yaw can range from -15 to +15.  the robot's angular velocity changes this value.
		float yaw = 0f;
		// the (+) assignments make the camera lead the robot while turning, and the (-) assignments make it lag behind
		if (robotScript.AngularVelocity < 0f) {
			yaw = Mathf.Max (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), -15f);
//			yaw = -Mathf.Max (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), -15f);
		} else if (robotScript.AngularVelocity > 0f) {
			yaw = Mathf.Min (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), 15f);
//			yaw = -Mathf.Min (15f * (robotScript.AngularVelocity / robotScript.MaxAngularVelocity), 15f);
		} else {
			yaw = 0f;
		}
		// lerp the camera's angle based on the pitch and yaw calculated above
		transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (pitch, yaw, 0f), Time.deltaTime * 2);

		// sink can range from 15.0 to 7.5.  the robot's height above the ground changes this angle from 0-40m.
		float sink = Mathf.Min (11 + 4 * (robotScript.GetHeight () / 40), 15);
		float creep = Mathf.Min (-7 + 5 * (robotScript.GetHeight () / 40), -2);
		transform.localPosition = Vector3.Lerp (transform.localPosition, new Vector3 (0f, sink, creep), Time.deltaTime);

//		Debug.Log (robotTrans.localRotation.z);
//		Debug.Log("pitch:\t" + pitch);						// show pitch angle
//		Debug.Log("yaw:\t" + yaw);							// show yaw angle
//		Debug.Log("roll:\t" + roll);						// show roll angle
//		Debug.Log("sink:\t" + sink);						// show sink level


	}
}
