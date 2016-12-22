using UnityEngine;
using System.Collections;

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
		fuelMat = fuelBar.GetComponent<MeshRenderer> ();

		hopBars = new GameObject[5];
		skipBars = new GameObject[5];
		for (int i = 1; i < 6; i++) {
			hopBars [i] = GameObject.Find ("HB-" + i);
			skipBars [i] = GameObject.Find ("SB-" + i);
		}

		robot = GameObject.Find ("Robot");
		robotScript = robot.GetComponent<PlayerMovement> ();
	}
	
	// Update is called once per frame
	void Update () {

		// adjust the fuel bar to reflect the percentage of fuel the robot has left
		fuelBar.transform.localScale = new Vector3(0.125f, 1.5f * (robotScript.Fuel / (robotScript.fuelCapacity * 1000)), 0.125f);
		fuelBar.transform.localPosition = new Vector3(-3.5f, 0.75f * (robotScript.Fuel / (robotScript.fuelCapacity * 1000)) - 1.5f, 3.0f);

		for (int i = 1; i < 6; i++) {
			// update the hop gauge based on the robot's current hopping power
			if (robotScript.HopPow >= i) {
				hopBars [i].SetActive (true);
			} else {
				hopBars [i].SetActive (false);
			}
			// update the skip gauge based on the robot's current skipping power
			if (robotScript.SkipPow >= i) {
				skipBars [i].SetActive (true);
			} else {
				skipBars [i].SetActive (false);
			}
		}
	}
}
