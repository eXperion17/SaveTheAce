using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayoutAdjuster : MonoBehaviour {
	GridLayoutGroup gridLayout;

	//private bool keepAspectRatio;

	private Vector2 baseResolution = new Vector2(1080, 1920);
	private Vector2 baseCellSize;
	public bool debugMode;

	// Use this for initialization
	void Start () {
		if (!gridLayout)
			gridLayout = GetComponent<GridLayoutGroup>();

		baseCellSize = gridLayout.cellSize;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(Screen.currentResolution.width + "  /  " + baseResolution.y);
		/*int longest = (Screen.currentResolution.width > Screen.currentResolution.height) ? Screen.currentResolution.width : Screen.currentResolution.height;
		float ratio = longest / baseResolution.y;

		if (debugMode) {
			Debug.Log(longest + " / " + baseResolution.y);
		}
		gridLayout.cellSize = baseCellSize * ratio;*/
	}
}
