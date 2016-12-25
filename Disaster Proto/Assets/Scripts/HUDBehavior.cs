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
	private MeshRenderer[] hopMeshes;
	private MeshRenderer[] skipMeshes;
	private MeshRenderer hopOneMat;
	private MeshRenderer hopTwoMat;
	private MeshRenderer hopThreeMat;
	private MeshRenderer hopFourMat;
	private MeshRenderer hopFiveMat;
	private MeshRenderer skipOneMat;
	private MeshRenderer skipTwoMat;
	private MeshRenderer skipThreeMat;
	private MeshRenderer skipFourMat;
	private MeshRenderer skipFiveMat;
	private Material[] hopMats;
	private Material[] skipMats;
	public Material hopFive;
	public Material hopFour;
	public Material hopThree;
	public Material hopTwo;
	public Material hopOne;
	public Material skipFive;
	public Material skipFour;
	public Material skipThree;
	public Material skipTwo;
	public Material skipOne;
	public Material empty;

	public GameObject robot;
	private PlayerMovement robotScript;
	private Dictionary<string, Material> matDict;

	private int fuelTimer = 0;
	private bool blinkerState = true;

	// Use this for initialization
	void Start () {

		hopBars = new GameObject[5];
		skipBars = new GameObject[5];
		for (int i = 0; i < 5; i++) {
			hopBars [i] = GameObject.Find ("HB-" + (i + 1));
			skipBars [i] = GameObject.Find ("SB-" + (i + 1));
		}
		hopMeshes = new MeshRenderer[5];
		skipMeshes = new MeshRenderer[5];
		hopMats = new Material[5];
		skipMats = new Material[5];

		robotScript = robot.GetComponent<PlayerMovement> ();
		matDict = new Dictionary<string, Material> ();
		RegisterMaterials ();

	}
	
	// Update is called once per frame
	void Update () {

		// adjust the fuel bar to reflect the percentage of fuel the robot has left
		fuelPercentage = robotScript.Fuel / robotScript.FuelCapacity;
		if (fuelPercentage >= 0.67f) {
			fuelMaterial.material = highFuel;
		} else if (0.67f > fuelPercentage && fuelPercentage > 0.33f) {
			fuelMaterial.material = midFuel;
		} else if (0.33f >= fuelPercentage && fuelPercentage > 0.00f) {
			fuelMaterial.material = lowFuel;
			if (fuelTimer < fuelPercentage * 100) {
				++fuelTimer;
			} else {
				fuelBar.SetActive (blinkerState);
				blinkerState = !blinkerState;
				fuelTimer = 0;
			}
		} else {
			fuelMaterial.material = empty;
		}
			
		fuelBar.transform.localPosition = new Vector3(-0.33f, 0.5f * fuelPercentage - 0.49f, -1f);
		fuelBar.transform.localScale = new Vector3(0.25f, fuelPercentage, 6f);

		for (int i = 0; i < 5; i++) {
			// update the hop gauge based on the robot's current hopping power
			if (robotScript.HopPow >= (i + 1)) {
				hopMeshes[i].material = hopMats[i];
			} else {
				hopMeshes[i].material = empty;
			}
			// update the skip gauge based on the robot's current skipping power
			if (robotScript.SkipPow >= (i + 1)) {
				skipMeshes[i].material = skipMats[i];
			} else {
				skipMeshes[i].material = empty;
			}
		}

	}

	void RegisterMaterials() {

		// assign fuel
		fuelMaterial = fuelBar.GetComponent<MeshRenderer> ();

		// set the active states of each hop/skip bar
		for (int i = 0; i < 5; i++) {
			hopMeshes [i] = hopBars [i].GetComponent<MeshRenderer> ();
			skipMeshes [i] = skipBars [i].GetComponent<MeshRenderer> ();
			// deactivate bars higher than the robot's jump stat
			if (robotScript.jump >= (i + 1)) {
				hopBars [i].SetActive (true);
				skipBars [i].SetActive (true);
			} else {
				hopBars [i].SetActive (false);
				skipBars [i].SetActive (false);
			}
		}
			
		// set the positions and dimensions of eac hop/skip bar
		switch (robotScript.jump) {
		case 1:
			hopBars[0].transform.localPosition = new Vector3 (0f, 0.00f, 0f);
			hopBars[0].transform.localScale = new Vector3 (1f, 1.02f, 1f);
			skipBars[0].transform.localPosition = new Vector3 (0f, 0.00f, 0f);
			skipBars[0].transform.localScale = new Vector3 (1f, 1.02f, 1f);
			break;
		case 2:
			hopBars[1].transform.localPosition = new Vector3 (0f,  0.26f, 0f);
			hopBars[0].transform.localPosition = new Vector3 (0f, -0.26f, 0f);
			hopBars[1].transform.localScale = new Vector3 (1f, 0.50f, 1f);
			hopBars[0].transform.localScale = new Vector3 (1f, 0.50f, 1f);
			skipBars[1].transform.localPosition = new Vector3 (0f,  0.26f, 0f);
			skipBars[0].transform.localPosition = new Vector3 (0f, -0.26f, 0f);
			skipBars[1].transform.localScale = new Vector3 (1f, 0.50f, 1f);
			skipBars[0].transform.localScale = new Vector3 (1f, 0.50f, 1f);
			break;
		case 3:
			hopBars[2].transform.localPosition = new Vector3 (0f,  0.345f, 0f);
			hopBars[1].transform.localPosition = new Vector3 (0f,  0.000f, 0f);
			hopBars[0].transform.localPosition = new Vector3 (0f, -0.345f, 0f);
			hopBars[2].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			hopBars[1].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			hopBars[0].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			skipBars[2].transform.localPosition = new Vector3 (0f,  0.345f, 0f);
			skipBars[1].transform.localPosition = new Vector3 (0f,  0.000f, 0f);
			skipBars[0].transform.localPosition = new Vector3 (0f, -0.345f, 0f);
			skipBars[2].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			skipBars[1].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			skipBars[0].transform.localScale = new Vector3 (1f, 0.33f, 1f);
			break;
		case 4:
			hopBars[3].transform.localPosition = new Vector3 (0f,  0.39f, 0f);
			hopBars[2].transform.localPosition = new Vector3 (0f,  0.13f, 0f);
			hopBars[1].transform.localPosition = new Vector3 (0f, -0.13f, 0f);
			hopBars[0].transform.localPosition = new Vector3 (0f, -0.39f, 0f);
			hopBars[3].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			hopBars[2].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			hopBars[1].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			hopBars[0].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			skipBars[3].transform.localPosition = new Vector3 (0f,  0.39f, 0f);
			skipBars[2].transform.localPosition = new Vector3 (0f,  0.13f, 0f);
			skipBars[1].transform.localPosition = new Vector3 (0f, -0.13f, 0f);
			skipBars[0].transform.localPosition = new Vector3 (0f, -0.39f, 0f);
			skipBars[3].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			skipBars[2].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			skipBars[1].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			skipBars[0].transform.localScale = new Vector3 (1f, 0.24f, 1f);
			break;
		case 5:
			hopBars[4].transform.localPosition = new Vector3 (0f,  0.4150f, 0f);
			hopBars[3].transform.localPosition = new Vector3 (0f,  0.2075f, 0f);
			hopBars[2].transform.localPosition = new Vector3 (0f,  0.0000f, 0f);
			hopBars[1].transform.localPosition = new Vector3 (0f, -0.2075f, 0f);
			hopBars[0].transform.localPosition = new Vector3 (0f, -0.4150f, 0f);
			hopBars[4].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			hopBars[3].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			hopBars[2].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			hopBars[1].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			hopBars[0].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			skipBars[4].transform.localPosition = new Vector3 (0f,  0.4150f, 0f);
			skipBars[3].transform.localPosition = new Vector3 (0f,  0.2075f, 0f);
			skipBars[2].transform.localPosition = new Vector3 (0f,  0.0000f, 0f);
			skipBars[1].transform.localPosition = new Vector3 (0f, -0.2075f, 0f);
			skipBars[0].transform.localPosition = new Vector3 (0f, -0.4150f, 0f);
			skipBars[4].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			skipBars[3].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			skipBars[2].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			skipBars[1].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			skipBars[0].transform.localScale = new Vector3 (1f, 0.19f, 1f);
			break;
		}

		// place hopBar materials into their array
		hopMats [0] = hopOne;
		hopMats [1] = hopTwo;
		hopMats [2] = hopThree;
		hopMats [3] = hopFour;
		hopMats [4] = hopFive;

		// place skipBar materials into their array
		skipMats [0] = skipOne;
		skipMats [1] = skipTwo;
		skipMats [2] = skipThree;
		skipMats [3] = skipFour;
		skipMats [4] = skipFive;

	}
		
}
