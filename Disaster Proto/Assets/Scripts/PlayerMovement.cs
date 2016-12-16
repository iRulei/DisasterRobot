using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float accel;
	public float thrust;
	public float hopStrength;
	public float hopBuild;
	public float skipStrength;
	public float skipBuild;

	public int fuelCapacity;
	private int fuel;

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

		fuel = fuelCapacity * 800;
	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}

		// only move if the robot is alive
		if (isAlive) {

			// accelerating forward on the ground
			if (Input.GetKey (KeyCode.W) && isGrounded && body.velocity.magnitude < speed) {
				body.AddRelativeForce (Vector3.forward * 15 * accel);
			} else if (Input.GetKey (KeyCode.S) && body.velocity.magnitude < (speed / 2)) {
				body.AddRelativeForce (Vector3.back * 15 * accel);
			}
			else if (Input.GetKey (KeyCode.LeftShift) && fuel > 0) {
				if (isGrounded && body.velocity.magnitude < speed) {
					body.AddRelativeForce (Vector3.forward * 15 * thrust);
				} else {
					body.AddRelativeForce (Vector3.forward * 5 * thrust);
				}
				fuel -= 2;
				Debug.Log (100 * fuel / (fuelCapacity * 800) + "%");
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
			if (Input.GetKey (KeyCode.LeftControl) && isGrounded) {
				if (skipPow >= skipStrength) {
					skipPow = skipStrength;
				} else {
					skipPow += 0.005f * skipBuild;
				}
			} else if (Input.GetKeyUp (KeyCode.LeftControl)) {
				Skip ();
				skipPow = 0f;
			}
		}
	} 

	// launches the robot straight into the air
	void Hop() {
		if (isGrounded) {
			Debug.Log ("hopping: " + hopPow);
			body.AddForce (Vector3.up * hopPow * 400);
			isGrounded = false;
		}
	}

	// launches the robot forward in an arc
	void Skip() {
		if (isGrounded) {
			Debug.Log ("skipping: " + skipPow);
			body.AddRelativeForce (new Vector3(0, skipPow * 300, skipPow * 600));
			isGrounded = false;
		}
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Ground") { isGrounded = true; }
	}
}
