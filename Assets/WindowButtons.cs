using UnityEngine;
using System.Collections;

public class WindowButtons : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		if(GUI.Button(new Rect(Screen.width-35f,0,35f,20f),"X"))
		{
			Application.Quit();
		}
	}
}
