﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float thrust;
	public float jump;
	public float efficiency;
	public int fuelCapacity;

	private float hopPow;
	private float skipPow;
	private int fuel;

	// COOLDOWNS
	private Dictionary<string, RobotAbility> ableDict;

	private bool isAlive = true;

	private Rigidbody body;
	BoxCollider robotCollider;
	private Vector3 turnVector;
	private float groundRay;

//	private int sceneNum;

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

		// sets up the robot's active parameters
		ableDict = new Dictionary<string, RobotAbility>();
		RegisterAbilities ();
		hopPow = 0;
		skipPow = 0;
		fuel = fuelCapacity * 500;
	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}

		// Useful debugging output about the robot's status
		//Debug.Log(body.velocity.z + "m/s");
		//Debug.Log(IsGrounded());
		Debug.Log("hopCD\t" + ableDict["hop"].IsReady + "\nskipCD\t" + ableDict["skip"].IsReady);

		// only move if the robot is alive
		if (isAlive) {

			if (!IsGrounded()) {
				if (body.velocity.z > 0) {
					body.AddRelativeForce (Vector3.back * 10);
				} else if (body.velocity.z < 0) {
					body.AddRelativeForce (Vector3.forward * 10);
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
				if (IsGrounded() && !Input.GetKey (KeyCode.LeftControl) && ableDict["hop"].IsReady) {
					// grow hopping power only if it's not maxed out
					if (hopPow >= jump) {
						hopPow = jump;
					} else {
						hopPow += (0.01f * efficiency) + 0.01f;
					}
					Debug.Log ("building hop...\t" + hopPow);
				// fire ventral thrusters if they are engaged with L-CTRL
				} else if (Input.GetKey (KeyCode.LeftControl) && fuel > 0 && body.velocity.y < (float)(thrust)) {
					body.AddRelativeForce (Vector3.up * (int)(2.5 * thrust + 150));
					fuel -= 2;
					//Debug.Log (100 * fuel / (fuelCapacity * 500) + "% (ventral thrusters at " + (int)(2.5 * thrust + 150) + ")");
				}
			// execute a hop when the SPACE key is released
			} else if (Input.GetKeyUp (KeyCode.Space) && !Input.GetKey (KeyCode.LeftControl)) {
				if (IsGrounded() && ableDict["hop"].IsReady) {
					Hop ();
				}
				hopPow = 0f;
			}


			// SKIPPING and ANTERIOR THRUSTERS
			if (Input.GetKey (KeyCode.LeftShift)) {
				// build up / execute a skip if the robot is grounded and thrusters are NOT engaged
				if (IsGrounded() && !Input.GetKey (KeyCode.LeftControl) && ableDict["skip"].IsReady) {
					// grow skipping power only if it's not maxed out
					if (skipPow >= jump) {
						skipPow = jump;
					} else {
						skipPow += (0.0005f * efficiency) + (0.025f * skipPow);
					}
					Debug.Log ("building skip...\t" + skipPow);
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
				if (IsGrounded() && ableDict["skip"].IsReady) {
					Skip ();
				}
				skipPow = 0f;
			}
		}
	} 

	// launches the robot straight into the air
	private void Hop() {
		Debug.Log ("Hopping (" + (400 * hopPow + 2400) + ")");
		body.AddForce (Vector3.up * (400 * hopPow + 2400));
		ableDict["hop"].ActivateCoolDown();
	}

	// launches the robot forward in an arc
	private void Skip() {
		Debug.Log ("Skipping (" + (1200 * skipPow + 2000) + "Z, " + (240 * skipPow + 400) + "Y)");
		body.AddRelativeForce (new Vector3(0, (240 * skipPow + 400), (1200 * skipPow + 2000)));
		ableDict["skip"].ActivateCoolDown();
	}

	bool IsGrounded() {
		return Physics.Raycast (transform.position, Vector3.down, (float)(groundRay + 0.1f));
	}

//	void OnCollisionEnter(Collision col) {
//		if (col.gameObject.tag == "Ground") { isGrounded = true; }
//	}

	private void RegisterAbilities () {
		Debug.Log ("registering abilities");
		ableDict.Add ("hop", new RobotAbility("hop", (3250 - efficiency * 250)));
		ableDict.Add ("skip", new RobotAbility ("skip", (3250 - efficiency * 250)));
	}

	/// <summary>
	/// A wrapper class for a robot ability, including a string name, a cooldown, and a boolean indicating whether the ability's has recovered from cooldown.
	/// </summary>
	protected class RobotAbility {

		private string name;
		private double interval;
		private Timer timer;
		private bool isReady;

		public string Name { get { return name; } }
		public bool IsReady { get { return isReady; } }

		/// <summary>
		/// Creates a new RobotAbility with the specified name and cooldown.
		/// </summary>
		/// <param name="inName">The name of the ability.</param>
		/// <param name="timerMS">The cooldown timer in milliseconds.</param>
		public RobotAbility(string inName, double cooldown) {
			name = inName;
			interval = cooldown;
			timer = new Timer (interval);
			timer.Elapsed += ReadyAbility;
			isReady = true;
		}

		public void ActivateCoolDown() {
			isReady = false;
			timer.Start ();
		}

		public void ReadyAbility(object sender, ElapsedEventArgs e) {
			isReady = true;
			timer = new Timer (interval);
			timer.Elapsed += ReadyAbility;
		}
	}
}
