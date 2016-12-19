using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float thrust;
	public float hop;
	public float skip;
	public float efficiency;

	public int fuelCapacity;
	private int fuel;

	private bool isGrounded = false;
	private bool isAlive = true;

	private float hopPow;
	private float skipPow;
	private Vector3 turnVector;

//	private int sceneNum;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		
		// fetches the current level number for reference later
//		sceneNum = SceneManager.GetActiveScene ().buildIndex;

		// initializes the robot's velocity and heading
		body = GetComponent<Rigidbody> ();
		body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		turnVector = new Vector3 (0, (float)(0.70 + 0.20 * efficiency), 0);

		fuel = fuelCapacity * 500;
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

			// use W and S to move forward and back, limited by maximum speed
			if (Input.GetKey (KeyCode.W) && isGrounded && body.velocity.magnitude < (speed + 4)) {
				body.AddRelativeForce (Vector3.forward * (5 * efficiency + 150));
			} else if (Input.GetKey (KeyCode.S) && isGrounded && body.velocity.magnitude < (speed + 2)) {
				body.AddRelativeForce (Vector3.back * (5 * efficiency + 150));
			}


			// TURNING LEFT and RIGHT
			if (Input.GetKey (KeyCode.A)) {
				if (isGrounded) {
					transform.Rotate (-turnVector, Space.Self);
				} else {
					body.AddRelativeTorque (0, -((5 + thrust) / 20), 0);
				}
			} else if (Input.GetKey (KeyCode.D)) {
				if (isGrounded) {
					transform.Rotate (turnVector, Space.Self);
				} else {
					body.AddRelativeTorque (0, ((5 + thrust) / 20), 0);
				}
			}


			// HOPPING and VENTRAL THRUSTERS
			if (Input.GetKey (KeyCode.Space)) {
				// build up / execute a hop if the robot is grounded and thrusters are NOT engaged
				if (isGrounded && !Input.GetKey (KeyCode.LeftControl)) {
					// grow hopping power only if it's not maxed out
					if (hopPow >= hop) {
						hopPow = hop;
					} else {
						hopPow += (0.01f * efficiency) + 0.01f;
					}
					Debug.Log ("hop: " + hopPow);
				// fire ventral thrusters if they are engaged with L-CTRL
				} else if (Input.GetKey (KeyCode.LeftControl) && fuel > 0 && body.velocity.y < (float)(thrust)) {
					body.AddRelativeForce (Vector3.up * (int)(2.5 * thrust + 150));
					fuel -= 2;
					isGrounded = false;
					Debug.Log (100 * fuel / (fuelCapacity * 500) + "% (ventral thrusters at " + (int)(2.5 * thrust + 150) + ")");
				}
			// execute a hop when the SPACE key is released
			} else if (Input.GetKeyUp (KeyCode.Space) && !Input.GetKey (KeyCode.LeftControl)) {
				if (isGrounded) {
					Hop ();
				}
				hopPow = 0f;
			}


			// SKIPPING and ANTERIOR THRUSTERS
			if (Input.GetKey (KeyCode.LeftShift)) {
				// build up / execute a skip if the robot is grounded and thrusters are NOT engaged
				if (isGrounded && !Input.GetKey (KeyCode.LeftControl)) {
					// grow skipping power only if it's not maxed out
					if (skipPow >= skip) {
						skipPow = skip;
					} else {
						skipPow += (0.005f * efficiency) + 0.005f;
					}
					Debug.Log ("skip: " + skipPow);
				// fire anterior thrusters if they are engaged with L-CTRL
				} else if (Input.GetKey (KeyCode.LeftControl) && fuel > 0 && body.velocity.z < (float)(thrust)) {
					if (isGrounded) {
						body.AddRelativeForce (Vector3.forward * (int)(10 * thrust + 150));
					} else {
						body.AddRelativeForce (Vector3.forward * (int)(5 * thrust));
					}
					fuel -= 1;
					Debug.Log (100 * fuel / (fuelCapacity * 500) + "% (anterior thrusters at " + (int)(10 * thrust + 150) + ")");
				}
			// execute a skip when the L-SHIFT key is released
			} else if (Input.GetKeyUp (KeyCode.LeftShift) && !Input.GetKey (KeyCode.LeftControl)) {
				if (isGrounded) {
					Skip ();
				}
				skipPow = 0f;
			}
		}
	} 

	// launches the robot straight into the air
	void Hop() {
		Debug.Log ("Hopping (" + (400 * hopPow + 2400) + ")");
		body.AddForce (Vector3.up * (400 * hopPow + 2400));
		isGrounded = false;
	}

	// launches the robot forward in an arc
	void Skip() {
		Debug.Log ("Skipping (" + (500 * skipPow + 3000) + ")");
		body.AddRelativeForce (new Vector3(0, (250 * skipPow + 1500), (500 * skipPow + 3000)));
		isGrounded = false;
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.tag == "Ground") { isGrounded = true; }
	}
}
