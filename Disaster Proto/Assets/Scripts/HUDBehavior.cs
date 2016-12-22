using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

	public GameObject fuelBar;
	public MeshRenderer fuelMat;

	public GameObject[] hopBars;
	public MeshRenderer hopMat;

	public GameObject[] skipBars;
	public MeshRenderer skipMat;

	public GameObject robot;
	PlayerMovement robotScript;

	// Use this for initialization
	void Start () {
		fuelBar = GameObject.Find("Fuel Indicator");

		hopBars = new GameObject[5];
		skipBars = new GameObject[5];
		for (int i = 0; i < 5; i++) {
			hopBars [i] = GameObject.Find ("HB-" + (i + 1));
			skipBars [i] = GameObject.Find ("SB-" + (i + 1));
		}

		robot = GameObject.Find ("Robot");
		robotScript = robot.GetComponent<PlayerMovement> ();
	}
	
	// Update is called once per frame
	void Update () {

		// adjust the fuel bar to reflect the percentage of fuel the robot has left
		fuelBar.transform.localScale = new Vector3(0.125f, 1.5f * (robotScript.Fuel / (robotScript.fuelCapacity * 1000)), 0.125f);
		fuelBar.transform.localPosition = new Vector3(-3.5f, 0.75f * (robotScript.Fuel / (robotScript.fuelCapacity * 1000)) - 1.5f, 3.0f);

		for (int i = 0; i < 5; i++) {
			// update the hop gauge based on the robot's current hopping power
			if (robotScript.HopPow >= (i + 1)) {
				hopBars [i].SetActive (true);
			} else {
				hopBars [i].SetActive (false);
			}
			// update the skip gauge based on the robot's current skipping power
			if (robotScript.SkipPow >= (i + 1)) {
				skipBars [i].SetActive (true);
			} else {
				skipBars [i].SetActive (false);
			}
		}
	}
}
