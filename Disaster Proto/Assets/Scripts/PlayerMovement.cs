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
	public float fuelCapacity;

	private float xSpeed;
	private float ySpeed;
	private float zSpeed;
	private float hopPow;
	private float skipPow;
	private float fuel;
	private float fuelTank;

	private List<string> asmWheels = new List<string>() { "WHEELS_CART", "WHEELS_BALL" };
	private string currentWheels;
	private List<string> asmThrusters = new List<string>() { "THRUSTERS_SAGGITAL", "THRUSTERS_LATERAL", "THRUSTERS_AXIAL" };
	private string currentThrusters;
	private bool canTilt;

	public float HopPow { get { return hopPow; } }
	public float SkipPow { get { return skipPow; } }
	public float Fuel { get { return fuel; } }
	public float FuelTank { get { return fuelTank; } }

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
		fuelTank = 1000 * fuelCapacity + 3000;
		fuel = (float)fuelTank;

		// DEV EQUIPMENT
		currentWheels = "WHEELS_BALL";
		currentThrusters = "THRUSTERS_AXIAL";

	}
	
	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}
			
		// Useful debugging output about the robot's status
		//Debug.Log("x:\t" + xSpeed + "m/s");																	// show lateral velocity
		Debug.Log("y:\t" + ySpeed + "m/s");																	// show vertical velocity
		Debug.Log("z:\t" + zSpeed + "m/s");																	// show forward velocity
		//Debug.Log(body.angularVelocity);																// show angular velocity
		//Debug.Log(IsGrounded());																		// show grounded status
		//Debug.Log(GetHeight());																		// show distance to ground
		//Debug.Log(100 * (fuel / (fuelCapacity * 1000)) + "%");										// show fuel percentage
		//Debug.Log("hopCD\t" + ableDict["hop"].IsReady + "\nskipCD\t" + ableDict["skip"].IsReady);		// show hop and skip cooldowns

		xSpeed = transform.InverseTransformDirection (body.velocity).x;
		ySpeed = transform.InverseTransformDirection (body.velocity).y;
		zSpeed = transform.InverseTransformDirection (body.velocity).z;

		// only move if the robot is alive
		if (isAlive) {

			// GROUND CONTROLS
			if (IsGrounded ()) {

				// moving FORWARD and BACK on mechanical wheels
				if (currentWheels == "WHEELS_BALL" || currentWheels == "WHEELS_CART") {
					if (Input.GetKey (KeyCode.W) && zSpeed < (speed + 4)) {
						body.AddRelativeForce (Vector3.forward * (5 * efficiency + 150));
					} else if (Input.GetKey (KeyCode.S) && -zSpeed < (speed + 2)) {
						body.AddRelativeForce (Vector3.back * (5 * efficiency + 150));
					}
				}

				// strafing LEFT and RIGHT on mechanical wheels
				if (currentWheels == "WHEELS_BALL") {
					if (Input.GetKey (KeyCode.A) && -xSpeed < (speed + 2)) {
						body.AddRelativeForce (Vector3.left * (5 * efficiency + 150));
					} else if (Input.GetKey (KeyCode.D) && xSpeed < (speed + 2)) {
						body.AddRelativeForce (Vector3.right * (5 * efficiency + 150));
					}
				}

				// turning CCW and CW on mechanical wheels
				if (Input.GetKey (KeyCode.Q)) {
					transform.Rotate (new Vector3(0f, -(0.10f * efficiency + 0.50f), 0f), Space.Self);
				} else if (Input.GetKey (KeyCode.E)) {
					transform.Rotate (new Vector3(0f, (0.10f * efficiency + 0.50f), 0f), Space.Self);
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

				// stablize the robot's forward-backward movement
				if (zSpeed > 0) {
					body.AddRelativeForce (Vector3.back * 10);
				} else if (zSpeed < 0) {
					body.AddRelativeForce (Vector3.forward * 2);
				}

				// stabilize the robot's left-right movement
				if (xSpeed > 0) {
					body.AddRelativeForce (Vector3.left * 5);
				} else if (xSpeed < 0) {
					body.AddRelativeForce (Vector3.right * 5);
				}

				// stabilize the robot's spin
				if (body.angularVelocity.y > 0) {
					body.AddRelativeTorque (0.0f, -(0.025f * thrust + 0.10f), 0.0f);
				} else if (body.angularVelocity.y < 0) {
					body.AddRelativeTorque (0.0f, (0.025f * thrust + 0.10f), 0.0f);
				}

				// moving FORWARD and BACK with saggital thrusters
				if (currentThrusters == "THRUSTERS_AXIAL" || currentThrusters == "THRUSTERS_SAGGITAL") {
					if (Input.GetKey (KeyCode.W)) {

					} else if (Input.GetKey (KeyCode.S)) {

					}
				}

				// strafing LEFT and RIGHT with lateral thrusters
				if (currentThrusters == "THRUSTERS_AXIAL" || currentThrusters == "THRUSTERS_LATERAL") {
					if (Input.GetKey (KeyCode.A)) {

					} else if (Input.GetKey (KeyCode.D)) {

					}
				}

				// rotating CCW and CW around the y-axis
				if (Input.GetKey (KeyCode.Q)) {
					// hold Q to rotate counter-clockwise (uses 1-2 fuel per tick)
					body.AddRelativeTorque (0, -(0.05f * thrust + 0.25f), 0);
					fuel -= (0.2f * thrust + 1.0f);
				} else if (Input.GetKey (KeyCode.E)) {
					// hold E to rotate clockwise (uses 1-2 fuel per tick)
					body.AddRelativeTorque (0, (0.05f * thrust + 0.25f), 0);
					fuel -= (0.2f * thrust + 1.0f);
				}
			}

			// engage and fire blast thrusters (will work regardless of IsGrounded())
			if (Input.GetKey (KeyCode.LeftControl) && fuel > 0) {
				// hold LeftControl to engage blast thrusters
				if (Input.GetKey (KeyCode.Space) && ySpeed < (0.5f * thrust + 2.5f)) {
					// hold Space to fire ventral blast thruster (uses 7-10 fuel per tick)
					body.AddRelativeForce (Vector3.up * (int)(5 * thrust + 225));
					fuel -= (0.6f * thrust + 7.0f);
				} else if (Input.GetKey (KeyCode.LeftShift) && zSpeed < (1.25 * thrust + 5.25f)) {
					// hold LeftShift to fire anterior blast thruster (uses 7-10 fuel per tick)
					body.AddRelativeForce (Vector3.forward * (int)(5 * thrust + 75));
					fuel -= (0.6f * thrust + 7.0f);
				}
			}

			// DEV CHEATS
			if (Input.GetKeyUp (KeyCode.F)) {
				fuel = fuelCapacity * fuelTank;
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
