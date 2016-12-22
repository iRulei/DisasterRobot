using UnityEngine;
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

	private float zSpeed;
	private float hopPow;
	private float skipPow;
	private float fuel;

	private bool canTilt;

	public float ZSpeed { get { return zSpeed; } }
	public float HopPow { get { return hopPow; } }
	public float SkipPow { get { return skipPow; } }
	public float Fuel { get { return fuel; } }

	// COOLDOWNS
	private Dictionary<string, RobotAbility> ableDict;

	private bool isAlive = true;

	private bool hasGyro = true;

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
		body.maxAngularVelocity = (0.1f * thrust + 1.0f);
		robotCollider = gameObject.GetComponent<BoxCollider> ();
		groundRay = robotCollider.bounds.extents.y;
		turnVector = new Vector3 (0, (float)(0.20 * efficiency + 0.70), 0);

		// sets up the robot's active parameters
		ableDict = new Dictionary<string, RobotAbility>();
		RegisterAbilities ();
		hopPow = 0;
		skipPow = 0;
		fuel = fuelCapacity * 1000;
	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}
			
		// Useful debugging output about the robot's status
		//Debug.Log(zSpeed + "m/s");																	// show forward velocity
		//Debug.Log(body.angularVelocity);																// show angular velocity
		//Debug.Log(IsGrounded());																		// show grounded status
		Debug.Log(GetHeight());																		// show distance to ground
		//Debug.Log(100 * (fuel / (fuelCapacity * 1000)) + "%");										// show fuel percentage
		//Debug.Log("hopCD\t" + ableDict["hop"].IsReady + "\nskipCD\t" + ableDict["skip"].IsReady);		// show hop and skip cooldowns

		zSpeed = transform.InverseTransformDirection (body.velocity).z;

		// only move if the robot is alive
		if (isAlive) {

			// GROUND CONTROLS
			if (IsGrounded ()) {

				// moving FORWARD and BACK on mechanical wheels
				if (Input.GetKey (KeyCode.W) && zSpeed < (speed + 4)) {
					body.AddRelativeForce (Vector3.forward * (5 * efficiency + 150));
				} else if (Input.GetKey (KeyCode.S) && -zSpeed < (speed + 2)) {
					body.AddRelativeForce (Vector3.back * (5 * efficiency + 150));
				}

				// turning LEFT and RIGHT on mechanical wheels
				if (Input.GetKey (KeyCode.A)) {
					transform.Rotate (-turnVector, Space.Self);
				} else if (Input.GetKey (KeyCode.D)) {
					transform.Rotate (turnVector, Space.Self);
				}

				// building up and executing a HOP
				if (!Input.GetKey (KeyCode.LeftControl) && ableDict ["hop"].IsReady) {
					if (Input.GetKey (KeyCode.Space)) {
						if (hopPow >= jump) {
							hopPow = jump;
						} else {
							hopPow += (0.0015f * efficiency) + (0.015f * hopPow);
						}
					} else if (Input.GetKeyUp (KeyCode.Space)) {
						hopPow = (int)hopPow;
						Hop ();
						hopPow = 0f;
					}
				}

				// building up and executing a SKIP
				if (!Input.GetKey (KeyCode.LeftControl) && ableDict ["skip"].IsReady) {
					if (Input.GetKey (KeyCode.LeftShift)) {
						if (skipPow >= jump) {
							skipPow = jump;
						} else {
							skipPow += (0.005f * efficiency) + 0.01f;
						}
					} else if (Input.GetKeyUp (KeyCode.LeftShift)) {
						skipPow = (int)skipPow;
						Skip ();
						skipPow = 0f;
					}
				}

			// AIRBORNE CONTROLS
			} else {
				hopPow = 0f;
				skipPow = 0f;

				// stablize the robot's rotation and forward-backward movement
				if (zSpeed > 0) {
					body.AddRelativeForce (Vector3.back * 10);
				} else if (zSpeed < 0) {
					body.AddRelativeForce (Vector3.forward * 2);
				}

				// rotate CW and CCW around the y-axis
				if (Input.GetKey (KeyCode.A)) {
					body.AddRelativeTorque (0, -((5 + thrust) / 20), 0);
					fuel -= 1;
				} else if (Input.GetKey (KeyCode.D)) {
					body.AddRelativeTorque (0, ((5 + thrust) / 20), 0);
					fuel -= 1;
				}
			}

			// maneuver with thrusters
			if (Input.GetKey (KeyCode.LeftControl) && fuel > 0) {
				if (Input.GetKey (KeyCode.Space) && body.velocity.y < (float)(thrust)) {
					body.AddRelativeForce (Vector3.up * (int)(2.5 * thrust + 150));
					fuel -= 5;
				} else if (Input.GetKey (KeyCode.LeftShift) && zSpeed < (float)(thrust + 4)) {
					body.AddRelativeForce (Vector3.forward * (int)(5 * thrust + 25));
					fuel -= 3f;
				}
			}

			// DEV CHEATS
			if (Input.GetKeyUp (KeyCode.F)) {
				fuel = fuelCapacity * 1000;
			}
		}
	}

	// PHYSICAL STATUS CHECKS

	public bool IsGrounded() {
		return Physics.Raycast (transform.position, Vector3.down, (float)(groundRay + 0.1f));
	}

	public float GetHeight() {
		RaycastHit[] ground = Physics.RaycastAll (transform.position, Vector3.down);
		if (ground.Length == 0) {
			return float.MaxValue;
		} else if (ground [0].GetType() != null) {
			return ground [0].distance;
		}
		return 0f;
	}

	void OnCollisionEnter(Collision col) {
				
	}

	private void RegisterAbilities () {
		ableDict.Add ("hop", new RobotAbility("hop", (1125 - efficiency * 125)));
		ableDict.Add ("skip", new RobotAbility ("skip", (1125 - efficiency * 125)));
	}

	// launches the robot straight into the air
	private void Hop() {
		if (hopPow > 0) {
			body.AddForce (Vector3.up * (500 * hopPow + 2500));
		}
		ableDict["hop"].ActivateCoolDown();
	}

	// launches the robot forward in an arc
	private void Skip() {
		if (skipPow > 0) {
			body.AddRelativeForce (new Vector3 (0, (200 * skipPow + 1300), (750 * skipPow + 3250)));
		}
		ableDict["skip"].ActivateCoolDown();
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
			timer.Elapsed += (object sender, ElapsedEventArgs e) => { timer.Stop(); isReady = true; };
			isReady = true;
		}

		/// <summary>
		/// Flags the ability as not ready and starts its cooldown timer.
		/// </summary>
		public void ActivateCoolDown() {
			isReady = false;
			timer.Start ();
		}
	}
}
