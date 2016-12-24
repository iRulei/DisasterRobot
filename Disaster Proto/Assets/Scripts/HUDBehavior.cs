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
	public Material hopOne;
	public Material hopTwo;
	public Material hopThree;
	public Material hopFour;
	public Material hopFive;
	public Material skipOne;
	public Material skipTwo;
	public Material skipThree;
	public Material skipFour;
	public Material skipFive;
	public Material empty;

	public GameObject robot;
	private PlayerMovement robotScript;
	private Dictionary<string, Material> matDict;

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
		} else {
			fuelMaterial.material = lowFuel;
		}

		fuelBar.transform.localPosition = new Vector3(-2.85f, 0.7375f * fuelPercentage - 1.475f, 2.8f);
		fuelBar.transform.localScale = new Vector3(0.125f, 1.5f * fuelPercentage, 0.0625f);

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

		fuelMaterial = fuelBar.GetComponent<MeshRenderer> ();

		for (int i = 0; i < 5; i++) {
			hopMeshes [i] = hopBars [i].GetComponent<MeshRenderer> ();	
			if (robotScript.jump >= (i + 1)) {
				hopBars [i].SetActive (true);
			} else {
				hopBars [i].SetActive (false);
			}

			skipMeshes [i] = skipBars [i].GetComponent<MeshRenderer> ();
			if (robotScript.jump >= (i + 1)) {
				skipBars [i].SetActive (true);
			} else {
				skipBars [i].SetActive (false);
			}
		}

		hopMats [0] = hopOne;
		hopMats [1] = hopTwo;
		hopMats [2] = hopThree;
		hopMats [3] = hopFour;
		hopMats [4] = hopFive;

		skipMats [0] = skipOne;
		skipMats [1] = skipTwo;
		skipMats [2] = skipThree;
		skipMats [3] = skipFour;
		skipMats [4] = skipFive;

	}
}
