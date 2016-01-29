using UnityEngine;
using System.Collections;

public class ClickDiverterSwitchVertical : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnMouseDown(){
		if(gameObject.transform.parent.GetComponent<VerticalDiverter>().diverteron)
		{
			gameObject.transform.parent.GetComponent<VerticalDiverter>().diverteron=false;
			//gameObject.renderer.material.color = Color.red;
		}
		else
		{
			gameObject.transform.parent.GetComponent<VerticalDiverter>().diverteron=true;
			//gameObject.renderer.material.color = Color.green;
		}	
	}
		
}

