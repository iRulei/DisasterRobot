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
	private float groundRay;
	BoxCollider robotCollider;

//	private int sceneNum;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		
		// fetches the current level number for reference later
//		sceneNum = SceneManager.GetActiveScene ().buildIndex;

		// initializes the robot
		body = GetComponent<Rigidbody> ();
		body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		robotCollider = gameObject.GetComponent<BoxCollider> ();
		groundRay = robotCollider.bounds.extents.y;
		turnVector = new Vector3 (0, (float)(0.20 * efficiency + 0.70), 0);
		fuel = fuelCapacity * 500;
	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}

		//Debug.Log(body.velocity.z + "m/s");
		Debug.Log(IsGrounded());

		// only move if the robot is alive
		if (isAlive) {

			if (!IsGrounded()) {
				if (body.velocity.z > 0) {
					body.AddRelativeForce (Vector3.back * 10);
				} else if (body.velocity.z < 0) {
					body.AddRelativeForce (Vector3.forward * 2);
				}
			}

			// use W and S to move forward and back, limited by maximum speed
			if (Input.GetKey (KeyCode.W) && IsGrounded() && body.velocity.magnitude < (speed + 4)) {
				body.AddRelativeForce (Vector3.forward * (5 * efficiency + 150));
			} else if (Input.GetKey (KeyCode.S) && IsGrounded() && body.velocity.magnitude < (speed + 2)) {
				body.AddRelativeForce (Vector3.back * (5 * efficiency + 150));
			}


			// TURNING LEFT and RIGHT
			if (Input.GetKey (KeyCode.A)) {
				if (IsGrounded()) {
					transform.Rotate (-turnVector, Space.Self);
				} else {
					body.AddRelativeTorque (0, -((5 + thrust) / 20), 0);
				}
			} else if (Input.GetKey (KeyCode.D)) {
				if (IsGrounded()) {
					transform.Rotate (turnVector, Space.Self);
				} else {
					body.AddRelativeTorque (0, ((5 + thrust) / 20), 0);
				}
			}


			// HOPPING and VENTRAL THRUSTERS
			if (Input.GetKey (KeyCode.Space)) {
				// build up / execute a hop if the robot is grounded and thrusters are NOT engaged
				if (IsGrounded() && !Input.GetKey (KeyCode.LeftControl)) {
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
					//isGrounded = false;
					//Debug.Log (100 * fuel / (fuelCapacity * 500) + "% (ventral thrusters at " + (int)(2.5 * thrust + 150) + ")");
				}
			// execute a hop when the SPACE key is released
			} else if (Input.GetKeyUp (KeyCode.Space) && !Input.GetKey (KeyCode.LeftControl)) {
				if (IsGrounded()) {
					Hop ();
				}
				hopPow = 0f;
			}


			// SKIPPING and ANTERIOR THRUSTERS
			if (Input.GetKey (KeyCode.LeftShift)) {
				// build up / execute a skip if the robot is grounded and thrusters are NOT engaged
				if (IsGrounded() && !Input.GetKey (KeyCode.LeftControl)) {
					// grow skipping power only if it's not maxed out
					if (skipPow >= skip) {
						skipPow = skip;
					} else {
						skipPow += skipPow + 0.001f;
					}
					Debug.Log ("skip: " + skipPow);
				// fire anterior thrusters if they are engaged with L-CTRL
				} else if (Input.GetKey (KeyCode.LeftControl) && fuel > 0 && body.velocity.z < (float)(thrust)) {
					if (IsGrounded()) {
						body.AddRelativeForce (Vector3.forward * (int)(5 * thrust + 150));
					} else {
						body.AddRelativeForce (Vector3.forward * (int)(5 * thrust + 25));
					}
					fuel -= 1;
					//Debug.Log (100 * fuel / (fuelCapacity * 500) + "% fuel (anterior thrusters at " + (int)(10 * thrust + 150) + ")");
				}
			// execute a skip when the L-SHIFT key is released
			} else if (Input.GetKeyUp (KeyCode.LeftShift) && !Input.GetKey (KeyCode.LeftControl)) {
				if (IsGrounded()) {
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
		//isGrounded = false;
	}

	// launches the robot forward in an arc
	void Skip() {
		Debug.Log ("Skipping (" + (1000 * skipPow + 500) + ")");
		body.AddRelativeForce (new Vector3(0, (400 * skipPow + 200), (1000 * skipPow + 500)));
		//isGrounded = false;
	}

	bool IsGrounded() {
		return Physics.Raycast (transform.position, Vector3.down, (float)(groundRay + 0.1f));
	}

//	void OnCollisionEnter(Collision col) {
//		if (col.gameObject.tag == "Ground") { isGrounded = true; }
//	}
}
