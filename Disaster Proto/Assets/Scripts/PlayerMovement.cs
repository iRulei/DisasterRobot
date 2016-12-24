using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	// the robot itself
	private Rigidbody body;
	private BoxCollider robotCollider;

	// ROBOT STATS
	public float speed;
	public float thrust;
	public float jump;
	public float efficiency;
	public float fuelTank;

	// FIXED STATUS VARIABLES
	// kinematic
	private float maxLandSpeed;
	private float maxAirSpeed;
	private float maxTilt;
	private float maxSpin;
	// equipment-related
	private float boost;
	private float fuel;
	private float fuelCapacity;

	// DYNAMIC STATUS VARIABLES
	// kinematic
	private float xSpeed;
	private float ySpeed;
	private float zSpeed;
	private float xTilt;
	private float yTilt;
	private float zTilt;
	private Vector3 localEulers;
	// equipment-related
	private float hopPow = 0f;
	private float skipPow = 0f;
	// vital
	private bool isAlive = true;

	// these represent the player's inventory, and should later be moved to another script
	private List<string> asmWheels = new List<string>() { "WHEELS_CART", "WHEELS_BALL" };
	private List<string> asmThrusters = new List<string>() { "THRUSTERS_SAGGITAL", "THRUSTERS_LATERAL", "THRUSTERS_AXIAL", "THRUSTERS_GYROSCOPIC" };
	private List<string> asmMod = new List<string>() { };
	private string currentWheels;
	private string currentThrusters;
	private string currentMod;

	// PROPERTIES FOR EXTERNAL ACCESS
	public float Fuel { get { return fuel; } }
	public float FuelCapacity { get { return fuelCapacity; } }
	public float AngularVelocity { get { return body.angularVelocity.y; } }
	public float MaxAngularVelocity { get { if (body != null) { return body.maxAngularVelocity; } else { return 0.0f; } } }
	public float HopPow { get { return hopPow; } }
	public float SkipPow { get { return skipPow; } }

	// COOLDOWNS
	private Dictionary<string, RobotAbility> ableDict;




	// Use this for initialization
	void Start () {
		
		// sets up the robot
		body = GetComponent<Rigidbody> ();
		body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		body.maxAngularVelocity = (0.1f * thrust + 1.0f);
		robotCollider = gameObject.GetComponent<BoxCollider> ();

		// kinematic limitations
		maxLandSpeed = speed + 4f;					// 5.0 - 9.0   m/s	forward velocity while driving
		maxAirSpeed = 0.75f * thrust + 2.25f;		// 3.0 - 6.0   m/s	velocity while flying
		maxTilt = 1.25f * efficiency + 38.75f;		// 40  - 45  deg	tilt during gyro operation
		maxSpin = 0.1f * efficiency + 1.0f;			// 1.1 - 1.5 rad/s	angular velocity while flying
		boost = 12.5f * thrust + 37.5f;				// 50  - 100   N	force of axial thrusters while flying

		fuelCapacity = 1000 * fuelTank + 3000;
		fuel = (float)fuelCapacity;

		localEulers = Vector3.zero;

		ableDict = new Dictionary<string, RobotAbility>();
		RegisterAbilities ();



		// [!] DEV EQUIPMENT [!]
		currentWheels = "WHEELS_BALL";
		currentThrusters = "THRUSTERS_GYROSCOPIC";

	}




	// Update is called once per frame
	void Update () {
		
		// kill the robot if they've fallen into the void
		if (isAlive && body.position.y < -100) {
			Debug.Log ("killed by ABYSS");
			isAlive = false;
		}

		ApplyCheats ();
		DebugOutput ();

		// the robot's velocity on each axis relative to itself
		xSpeed = transform.InverseTransformDirection (body.velocity).x;
		ySpeed = transform.InverseTransformDirection (body.velocity).y;
		zSpeed = transform.InverseTransformDirection (body.velocity).z;
		// x and z are swapped to make them agree with the robot's local axes
		xTilt = transform.localRotation.eulerAngles.z;
		yTilt = transform.localRotation.eulerAngles.y;
		zTilt = transform.localRotation.eulerAngles.x;
		// the robot's current local rotational angles
		localEulers = transform.localRotation.eulerAngles;



		// [!] MAIN CONTROL ALGORITHM [!]
		// only control the robot if it's alive
		if (isAlive) {

			// GROUND CONTROLS
			if (IsGrounded ()) {

				// MOVE FORWARD and BACK
				if (currentWheels == "WHEELS_BALL" || currentWheels == "WHEELS_TANK" || currentWheels == "WHEELS_CART") {
					if (Input.GetKey (KeyCode.W) && zSpeed < maxLandSpeed) {
						body.AddRelativeForce (Vector3.forward * (5 * efficiency + 150));
					} else if (Input.GetKey (KeyCode.S) && -zSpeed < (maxLandSpeed - 2)) {
						body.AddRelativeForce (Vector3.back * (5 * efficiency + 150));
					}
				}
					
				// MOVE LEFT and RIGHT
				if (currentWheels == "WHEELS_BALL") {
					if (Input.GetKey (KeyCode.A) && -xSpeed < (maxLandSpeed - 2)) {
						body.AddRelativeForce (Vector3.left * (5 * efficiency + 150));
					} else if (Input.GetKey (KeyCode.D) && xSpeed < (maxLandSpeed - 2)) {
						body.AddRelativeForce (Vector3.right * (5 * efficiency + 150));
					}
				}
					
				// ROTATE CW and CCW
				if (Input.GetKey (KeyCode.Q) && -AngularVelocity < maxSpin) {
					if ((currentWheels == "WHEELS_TANK" && body.velocity.magnitude < (speed / 2)) || currentWheels != "WHEELS_TANK") {
						transform.Rotate (new Vector3(0f, -(0.10f * efficiency + 0.50f), 0f), Space.World);
					}
				} else if (Input.GetKey (KeyCode.E) && AngularVelocity < maxSpin) {
					if ((currentWheels == "WHEELS_TANK" && body.velocity.magnitude < (speed / 2)) || currentWheels != "WHEELS_TANK") {
						transform.Rotate (new Vector3(0f, (0.10f * efficiency + 0.50f), 0f), Space.World);
					}
				}
					
				// HOP (build up and execute)
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

				// SKIP (build up and execute)
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

				// STABILIZE Z-DRIFT
				if (zSpeed < 0) {
					body.AddRelativeForce (Vector3.forward * (5 * thrust + 10));
				} else {
					body.AddRelativeForce (Vector3.back * (5 * thrust + 10));
				}

				// STABILIZE X-DRIFT
				if (xSpeed < 0) {
					body.AddRelativeForce (Vector3.right * (5 * thrust + 10));
				} else {
					body.AddRelativeForce (Vector3.left * (5 * thrust + 10));
				}

				// STABILIZE Y-SPIN
				if (AngularVelocity > 0) {
					body.AddRelativeTorque (0.0f, -(0.025f * thrust + 0.175f), 0.0f);
				} else if (AngularVelocity < 0) {
					body.AddRelativeTorque (0.0f, (0.025f * thrust + 0.175f), 0.0f);
				}
										


				// ENGAGE AXIAL THRUSTERS
				if (Input.GetKey (KeyCode.LeftControl) && fuel > 0) {

					// MOVE FORWARD and BACK
					if ((currentThrusters == "THRUSTERS_AXIAL" || currentThrusters == "THRUSTERS_SAGGITAL")) {
						if (Input.GetKey (KeyCode.W)) {
							if (zSpeed < maxAirSpeed) {
								body.AddRelativeForce (Vector3.forward * boost);
							}
							fuel -= (0.2f * thrust + 2.0f);
						} else if (Input.GetKey (KeyCode.S)) {
							if (-zSpeed < maxAirSpeed) {
								body.AddRelativeForce (Vector3.back * boost);
							}
							fuel -= (0.2f * thrust + 2.0f);
						}
					}

					// MOVE LEFT and RIGHT
					if ((currentThrusters == "THRUSTERS_AXIAL" || currentThrusters == "THRUSTERS_LATERAL")) {
						if (Input.GetKey (KeyCode.A)) {
							if (-xSpeed < maxAirSpeed) {
								body.AddRelativeForce (Vector3.left * boost);
							}
							fuel -= (0.2f * thrust + 2.0f);
						} else if (Input.GetKey (KeyCode.D)) {
							if (xSpeed < maxAirSpeed) {
								body.AddRelativeForce (Vector3.right * boost);
							}
							fuel -= (0.2f * thrust + 2.0f);
						}
					}
					
					// ROTATE CW and CCW
					if (Input.GetKey (KeyCode.Q)) {
						// hold Q to rotate counter-clockwise (uses 1-2 fuel per tick)
						body.AddRelativeTorque (Vector3.down * (0.075f * thrust + 0.225f));
						fuel -= (0.2f * thrust + 1.0f);
					} else if (Input.GetKey (KeyCode.E)) {
						// hold E to rotate clockwise (uses 1-2 fuel per tick)
						body.AddRelativeTorque (Vector3.up * (0.075f * thrust + 0.225f));
						fuel -= (0.2f * thrust + 1.0f);
					}
				}



				// GYROSCOPIC THRUSTER CONTROL
				if (currentThrusters == "THRUSTERS_GYROSCOPIC") {
					if (Input.GetKey (KeyCode.W)) {
						// hold W to lean FORWARD												// WHEN YOU RETURN: the problem is that this Lerp pushes toward WORLD'S x-axis every time DESPITE starting from your LOCAL axes.
						if (zSpeed <= maxAirSpeed) {											//					you need to find a way to have this lerp command translate your LOCAL axes into WORLD axes.
							Vector3 forwardTempWaxing = localEulers;
							forwardTempWaxing.x += (maxTilt * (0.75f + (Mathf.Abs(0.25f * zSpeed) / maxAirSpeed)));
							forwardTempWaxing.y += 0f;
							forwardTempWaxing.z += 0f;
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (forwardTempWaxing), Time.deltaTime);
						} else {
							Vector3 forwardTempWaning = localEulers;
							forwardTempWaning.x += (maxTilt * (maxAirSpeed / Mathf.Abs (zSpeed)));
							forwardTempWaning.y += 0f;
							forwardTempWaning.z += 0f;
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (forwardTempWaning), Time.deltaTime);
						}
					} else if (Input.GetKey (KeyCode.S)) {
						// hold S to lean BACK
						if (-zSpeed <= maxAirSpeed) {
							Vector3 backTempWaxing = localEulers;
							backTempWaxing.x -= (maxTilt * (0.75f + (Mathf.Abs(0.25f * zSpeed) / maxAirSpeed)));
							backTempWaxing.y += 0f;
							backTempWaxing.z += 0f;
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (backTempWaxing), Time.deltaTime);
						} else {
							Vector3 backTempWaning = localEulers;
							backTempWaning.x -= (maxTilt * (maxAirSpeed / Mathf.Abs (zSpeed)));
							backTempWaning.y += 0f;
							backTempWaning.z += 0f;
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (backTempWaning), Time.deltaTime);
						}
					}

					if (Input.GetKey (KeyCode.A)) {
						// hold A to lean LEFT
						if (-xSpeed <= maxAirSpeed) {
							Vector3 leftTempWaxing = localEulers;
							leftTempWaxing.x += 0f;
							leftTempWaxing.y += 0f;
							leftTempWaxing.z += (maxTilt * (0.75f + (Mathf.Abs(0.25f * xSpeed) / maxAirSpeed)));
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (leftTempWaxing), Time.deltaTime);
						} else {
							Vector3 leftTempWaning = localEulers;
							leftTempWaning.x += 0f;
							leftTempWaning.y += 0f;
							leftTempWaning.z += (maxTilt * (maxAirSpeed / Mathf.Abs (xSpeed)));
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (leftTempWaning), Time.deltaTime);
						}
					} else if (Input.GetKey (KeyCode.D)) {
						// hold D to lean RIGHT
						if (xSpeed <= maxAirSpeed) {
							Vector3 rightTempWaxing = localEulers;
							rightTempWaxing.x += 0f;
							rightTempWaxing.y += 0f;
							rightTempWaxing.z -= (maxTilt * (0.75f + (Mathf.Abs(0.25f * xSpeed) / maxAirSpeed)));
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (rightTempWaxing), Time.deltaTime);
						} else {
							Vector3 rightTempWaning = localEulers;
							rightTempWaning.x += 0f;
							rightTempWaning.y += 0f;
							rightTempWaning.z -= (maxTilt * (maxAirSpeed / Mathf.Abs (xSpeed)));
							transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (rightTempWaning), Time.deltaTime);
						}
					}
				}
			}



			// STABILIZE THE ROBOT'S PITCH (zTilt) AND ROLL (xTilt) WITHOUT AFFECTING ITS YAW (yTilt)
			Vector3 stableTemp = localEulers;
			stableTemp.x *= 0;	// the robot's xTilt will lerp toward 0, evening out its roll
			stableTemp.y *= 1;	// the robot's yTilt will be unaffected, allowing it to rotate
			stableTemp.z *= 0;	// the robot's zTilt will lerp toward 0, evening out its pitch
			transform.localRotation = Quaternion.Lerp (transform.localRotation, Quaternion.Euler (stableTemp), Time.deltaTime);



			// ENGAGE BLAST THRUSTERS
			if (Input.GetKey (KeyCode.LeftControl) && fuel > 0) {
				// hold LeftControl to engage blast thrusters
				if (Input.GetKey (KeyCode.Space) && body.velocity.y < maxAirSpeed) {
					// hold Space to fire ventral blast thruster and move UPWARD 3-6m/s (uses 7-10 fuel per tick)
					body.AddRelativeForce (Vector3.up * (int)(5 * thrust + 225));
					fuel -= (0.6f * thrust + 7.0f);
				} else if (Input.GetKey (KeyCode.LeftShift) && zSpeed < (maxAirSpeed * 2)) {
					// hold LeftShift to fire anterior blast thruster and move FORWARD 6-12m/s (uses 7-10 fuel per tick)
					body.AddRelativeForce (Vector3.forward * (int)(5 * thrust + 75));
					fuel -= (0.6f * thrust + 7.0f);
				}
			}



			// DEV CHEATS
			if (Input.GetKeyUp (KeyCode.F)) {
				fuel = fuelCapacity;
			} else if (Input.GetKey (KeyCode.Alpha1)) {
				currentWheels = "WHEELS_TANK";
			} else if (Input.GetKey (KeyCode.Alpha2)) {
				currentWheels = "WHEELS_CART";
			} else if (Input.GetKey (KeyCode.Alpha3)) {
				currentWheels = "WHEELS_BALL";
			} else if (Input.GetKey (KeyCode.Z)) {
				currentThrusters = "THRUSTERS_LATERAL";
			} else if (Input.GetKey (KeyCode.C)) {
				currentThrusters = "THRUSTERS_SAGGITAL";
			} else if (Input.GetKey (KeyCode.X)) {
				currentThrusters = "THRUSTERS_AXIAL";
			} else if (Input.GetKey (KeyCode.G)) {
				currentThrusters = "THRUSTERS_GYROSCOPIC";
			}
		}

	}




	// PHYSICAL STATUS CHECKS //

	public bool IsGrounded() {
		return Physics.Raycast (transform.position, Vector3.down, (float)(robotCollider.bounds.extents.y + 0.1f));
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




	// ABILITIES //

	private void RegisterAbilities () {
		// Hop has a cooldown of 1000-500ms
		ableDict.Add ("hop", new RobotAbility("hop", (1125 - efficiency * 125)));
		// Skip has a cooldown of 1000-500ms
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



	// CHEATS AND DEBUGGING

	private void ApplyCheats() {
		fuel = fuelCapacity;																			// refill fuel each tick								
	}

	private void DebugOutput() {
//		Debug.Log("xSpeed:\t" + (int)xSpeed + "m/s");													// show lateral velocity
//		Debug.Log("ySpeed:\t" + (int)ySpeed + "m/s");													// show vertical velocity
//		Debug.Log("zSpeed:\t" + (int)zSpeed + "m/s");													// show forward velocity
//		Debug.Log("maxAir:\t" + (int)maxAirSpeed + "m/s");												// show maxAirSpeed
		Debug.Log("xTilt:\t" + (int)localEulers.x);																// show local X tilt
		Debug.Log("yTilt:\t" + (int)localEulers.y);																// show local Y tilt
		Debug.Log("zTilt:\t" + (int)localEulers.z);																// show local Z tilt
//		Debug.Log(angularVelocity);																		// show angular velocity
//		Debug.Log(IsGrounded());																		// show grounded status
//		Debug.Log(GetHeight());																			// show distance to ground
//		Debug.Log(100 * (fuel / fuelCapacity) + "%");													// show fuel percentage
//		Debug.Log(currentWheels + " | " + currentThrusters);											// show equipment
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
