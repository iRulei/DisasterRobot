using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float accel;
	public float hopStrength;
	public float hopBuild;
	public float skipStrength;
	public float skipBuild;

	private bool isGrounded = false;
	private bool isAlive = true;

	private float hopPow;
	private float skipPow;

	private Vector3 turnStrength;

//	private int sceneNum;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		
		// fetches the current level number for reference later
//		sceneNum = SceneManager.GetActiveScene ().buildIndex;

		// initializes the robot's velocity and heading
		body = GetComponent<Rigidbody> ();
		body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		turnStrength = new Vector3 (0, 0.75f, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (body.position.y == -10) {
			isAlive = false;
		}

		// stop the robot from moving if they're not alive
		if (!isAlive) {
			body.velocity *= 0;
		} else {

			// accelerating forward
			if (Input.GetKey (KeyCode.W)) {
				body.AddRelativeForce (Vector3.forward * accel);
			} else if (Input.GetKeyUp (KeyCode.W)) {

			} else if (Input.GetKey (KeyCode.S)) {
				body.AddRelativeForce (Vector3.back * accel);
			} else if (Input.GetKeyUp (KeyCode.S)) {

			}

			// turning left and right
			if (Input.GetKey (KeyCode.A)) {
				transform.Rotate (-turnStrength, Space.Self);
			} else if (Input.GetKey (KeyCode.D)) {
				transform.Rotate (turnStrength, Space.Self);
			}

			// hopping
			if (Input.GetKey (KeyCode.Space) && isGrounded) {
				if (hopPow >= hopStrength) {
					hopPow = hopStrength;
				} else {
					hopPow += 0.01f * hopBuild;
				}
			} else if (Input.GetKeyUp (KeyCode.Space)) {
				Hop ();
				hopPow = 0f;
			}

			// skipping
			if (Input.GetKey (KeyCode.LeftShift) && isGrounded) {
				if (skipPow >= skipStrength) {
					skipPow = skipStrength;
				} else {
					skipPow += 0.01f * skipBuild;
				}
			} else if (Input.GetKeyUp (KeyCode.LeftShift)) {
				Skip ();
				skipPow = 0f;
			}
		}
	} 

	void Accelerate(bool isForward) {
		if (isGrounded) {
			transform.Translate (Vector3.forward * speed);
		}
	}

	void Hop() {
		if (isGrounded) {
			Debug.Log ("hopping: " + hopPow);
			body.AddForce (Vector3.up * hopPow * 1000);
			isGrounded = false;
		}
	}

	void Skip() {
		if (isGrounded) {
			Debug.Log ("skipping: " + skipPow);
			body.AddRelativeForce (new Vector3(0, skipPow * 500, skipPow * 1000));
			isGrounded = false;
		}
	}

	void OnCollisionEnter(Collision col) {
		
		if (col.gameObject.tag == "Ground") { isGrounded = true; }
	}
}
