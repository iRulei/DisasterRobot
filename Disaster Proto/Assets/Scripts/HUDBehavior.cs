using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDBehavior : MonoBehaviour {

	public GameObject fuelBar;
	private MeshRenderer fuelMaterial;
	private float fuelPercentage;
	public Material highFuel;
	public Material midFuel;
	public Material lowFuel;

	private GameObject[] hopBars;
	private GameObject[] skipBars;

	public GameObject robot;
	private PlayerMovement robotScript;
	private Dictionary<string, Color> colorDict;

	// Use this for initialization
	void Start () {

		fuelMaterial = fuelBar.GetComponent<MeshRenderer> ();

		hopBars = new GameObject[5];
		skipBars = new GameObject[5];
		for (int i = 0; i < 5; i++) {
			hopBars [i] = GameObject.Find ("HB-" + (i + 1));
			skipBars [i] = GameObject.Find ("SB-" + (i + 1));
		}

		robotScript = robot.GetComponent<PlayerMovement> ();
		colorDict = new Dictionary<string, Color> ();

	}
	
	// Update is called once per frame
	void Update () {

		// adjust the fuel bar to reflect the percentage of fuel the robot has left
		fuelPercentage = robotScript.Fuel / (robotScript.fuelCapacity * 1000);
		if (fuelPercentage > 0.67f) {
			fuelMaterial.material = highFuel;
		} else if (0.67f > fuelPercentage && fuelPercentage >= 0.33f) {
			fuelMaterial.material = midFuel;
		} else {
			fuelMaterial.material = lowFuel;
		}

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

//	void RegisterHUDColors() {
//		colorDict.Add ("fuel_green", new Color (0.6250f, 1.0000f, 0.6250f));
//		colorDict.Add ("fuel_yellow", new Color (1.0000f, 1.0000f, 0.5000f));
//		colorDict.Add ("fuel_red", new Color (1.0000f, 0.1875f, 0.1875f));
//	}
}
