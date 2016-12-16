using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

	public float speed;
	public float hopStrength;
	public float skipStrength;
	private float hopBuild;
	private float skipBuild;

	private bool isGrounded = false;
	private bool isAlive = true;
	private Vector3 heading;

//	private int sceneNum;

	private Rigidbody body;

	// Use this for initialization
	void Start () {
		
		// fetches the current level number for reference later
//		sceneNum = SceneManager.GetActiveScene ().buildIndex;

		// initializes the robot's velocity and heading
		body = GetComponent<Rigidbody> ();
		body.velocity = new Vector3 (0, 0, 0);
		heading = transform.forward;
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

			body.rotation = new Quaternion (0, 0, 0, 0);

			if (Input.GetKey (KeyCode.Space) && isGrounded) {
				hopBuild += 0.01f;
			} else if (Input.GetKeyUp (KeyCode.Space)) {
				Hop ();
				hopBuild = 0f;
			}

			if (Input.GetKey (KeyCode.UpArrow)) {
				body.velocity = new Vector3 (0, 0, speed);
			} else if (Input.GetKeyUp (KeyCode.UpArrow)) {
				body.velocity = new Vector3 (0, 0, 0);
			}
		}
	}

	void Hop() {
		if (isGrounded) {
			body.AddForce (Vector3.up * hopStrength * hopBuild);
			isGrounded = false;
		}
	}

	void OnCollisionEnter(Collision col) {
		
		if (col.gameObject.tag == "Ground") { isGrounded = true; }
	}
}
